using System.Collections.Immutable;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Sprig.Codegen.IR;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

internal sealed class Emitter(IR_Program program) {

    public void LoadReferences(string moduleName, string[] references) {
        foreach (var reference in references) {
            try {
                var assembly = AssemblyDefinition.ReadAssembly(reference);
                referenceAssemblies.Add(assembly);
            } 
            catch (BadImageFormatException) {
                diagnostics.ReportInvalidReferenceLinking(reference);
            }
        }

        var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
        programAssembly = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);        
         
        var builtinTypes = new List<(TypeSymbol type, string metadataName)>() {
            (TypeSymbol.Any,     "System.Object"),
            (TypeSymbol.Void,    "System.Void"),
            (TypeSymbol.Boolean,    "System.Boolean"),
            (TypeSymbol.Int32,     "System.Int32"),
            (TypeSymbol.Double, "System.Double"),
            (TypeSymbol.String,  "System.String"),
        };

        TypeReference? ResolveType(string? typeName, string metadataName) {
            var foundTypes = referenceAssemblies
                .SelectMany(assembly => assembly.Modules)
                .SelectMany(module => module.Types)
                .Where(type => type.FullName == metadataName)
                .ToArray();

            if (foundTypes.Length == 1) {
                var typeReference = programAssembly.MainModule.ImportReference(foundTypes[0]);
                return typeReference;
            }

            else if (foundTypes.Length == 0)
                diagnostics.ReportRequiredTypeNotFound(typeName, metadataName);
            
            else
                diagnostics.ReportRequiredTypeAmbiguous(typeName, metadataName, foundTypes);

            return null;
        }
        
        MethodReference? ResolveMethod(string typeName, string methodName, string[] parameterTypeNames) {
            var foundTypes = referenceAssemblies
                .SelectMany(assembly => assembly.Modules)
                .SelectMany(module => module.Types)
                .Where(type => type.FullName == typeName)
                .ToArray();

            if (foundTypes.Length == 1) {
                var foundType = foundTypes[0];
                var methods = foundType.Methods.Where(method => method.Name == methodName);

                foreach (var method in methods) {
                    if (method.Parameters.Count != parameterTypeNames.Length) 
                        continue;
                    
                    var allParameteresMatch = true;

                    for (var i = 0; i < parameterTypeNames.Length; i++) {
                        if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i]) {
                            allParameteresMatch = false;
                            break;
                        }
                    }

                    if (!allParameteresMatch)
                        continue;

                    var methodReference = programAssembly.MainModule.ImportReference(method);
                    return methodReference;
                }

                diagnostics.ReportRequiredMethodNotFound(typeName, methodName, parameterTypeNames);
                return null;
            }

            else if (foundTypes.Length == 0)
                diagnostics.ReportRequiredTypeNotFound(null, typeName);
            
            else
                diagnostics.ReportRequiredTypeAmbiguous(null, typeName, foundTypes);

            return null;
        }

        foreach (var (typeSymbol, metadataName) in builtinTypes) {
            var type = ResolveType(typeSymbol.Name, metadataName);
            knownTypes.Add(typeSymbol, type);
        }

        resolvedMethods.Add("WriteLine",    ResolveMethod("System.Console", "WriteLine", ["System.Object"]));
        resolvedMethods.Add("ReadLine",     ResolveMethod("System.Console", "ReadLine", []));
        resolvedMethods.Add("Concat",       ResolveMethod("System.String", "Concat", ["System.String", "System.String"]));
        resolvedMethods.Add("ObjectEquals", ResolveMethod("System.Object", "Equals", ["System.Object", "System.Object"]));
        
        resolvedMethods.Add("ConvertToBoolean", ResolveMethod("System.Convert", "ToBoolean", ["System.Object"]));
        resolvedMethods.Add("ConvertToInt32",   ResolveMethod("System.Convert", "ToInt32", ["System.Object"]));
        resolvedMethods.Add("ConvertToDouble",  ResolveMethod("System.Convert", "ToDouble", ["System.Object"]));
        resolvedMethods.Add("ConvertToString",  ResolveMethod("System.Convert", "ToString", ["System.Object"]));
        
        randType = ResolveType(null, "System.Random");
        resolvedMethods.Add("RandCtor", ResolveMethod("System.Random", ".ctor", []));
        resolvedMethods.Add("RandNext", ResolveMethod("System.Random", "Next", ["System.Int32", "System.Int32"]));
    }

    public void Emit(string outputPath) {
        var objectType = knownTypes[TypeSymbol.Any];
        mainProgram = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
        programAssembly.MainModule.Types.Add(mainProgram);

        foreach (var header in program.Functions.Keys)
            EmitFunctionHeader(header);

        foreach (var (header, body) in program.Functions)
            EmitFunctionBody(header, body);

        if (program.MainFunction != null)
            programAssembly.EntryPoint = programMethods[program.MainFunction];
        
        programAssembly.Write(outputPath);
    }

    private void EmitFunctionHeader(FunctionSymbol header) {
        var functionType = knownTypes[header.Type];
        var method = new MethodDefinition(header.Name, MethodAttributes.Private | MethodAttributes.Static, functionType);

        foreach (var parameter in header.Parameters) {
            var parameterType = knownTypes[parameter.Type];
            var parameterDefinition = new ParameterDefinition(parameter.Name, ParameterAttributes.None, parameterType);
            method.Parameters.Add(parameterDefinition);
        }

        mainProgram.Methods.Add(method);
        programMethods.Add(header, method);
    }
    
    private void EmitFunctionBody(FunctionSymbol header, IR_BlockStatement body) {
        var method = programMethods[header];
        
        locals.Clear();
        labels.Clear();
        targetLabels.Clear();

        var processor = method.Body.GetILProcessor();
        foreach (var statement in body.Statements)
            EmitStatement(processor, statement);

        foreach (var target in targetLabels) {
            var instructionIndex = labels[target.Target];
            var targetInstruction = processor.Body.Instructions[instructionIndex];
            var instruction = processor.Body.Instructions[target.Index];

            instruction.Operand = targetInstruction;
        }

        method.Body.OptimizeMacros();
    }

    private void EmitStatement(ILProcessor processor, IR_Statement node) {
        switch (node.Kind) {
            case IR_NodeKind.VariableDeclaration:
                EmitVariableDeclaration(processor, (IR_VariableDeclaration)node);
                break;

            case IR_NodeKind.LabelStatement:
                EmitLabel(processor, (IR_LabelStatement)node);
                break;

            case IR_NodeKind.NopStatement:
                EmitNop(processor);
                break;

            case IR_NodeKind.GotoStatement:
                EmitGoto(processor, (IR_GotoStatement)node);
                break;

            case IR_NodeKind.ConditionalGotoStatement:
                EmitConditionalGoto(processor, (IR_ConditionalGotoStatement)node);
                break;

            case IR_NodeKind.ReturnStatement:
                EmitReturn(processor, (IR_ReturnStatment)node);
                break;

            case IR_NodeKind.ExpressionStatement:
                EmitExpressionStatement(processor, (IR_ExpressionStatement)node);
                break;
            
            default:
                throw new Exception($"Unexpected statement: {node.Kind}");
        }
    }

    private void EmitVariableDeclaration(ILProcessor processor, IR_VariableDeclaration node) {
        var type = knownTypes[node.Variable.Type];
        var variable = new VariableDefinition(type);
        
        locals.Add(node.Variable, variable);
        processor.Body.Variables.Add(variable);

        EmitExpression(processor, node.Initializer);
        processor.Emit(OpCodes.Stloc, variable);
    }

    private void EmitLabel(ILProcessor processor, IR_LabelStatement node) {
        labels.Add(node.Label, processor.Body.Instructions.Count);        
    }

    private static void EmitNop(ILProcessor processor){
        processor.Emit(OpCodes.Nop);
    }

    private void EmitGoto(ILProcessor processor, IR_GotoStatement node) {
        var targetLabel = new TargetLabel(processor.Body.Instructions.Count, node.Label); 
        targetLabels.Add(targetLabel);
        
        processor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
    }

    private void EmitConditionalGoto(ILProcessor processor, IR_ConditionalGotoStatement node) {
        EmitExpression(processor, node.Condition);
        var opCode = node.Jump ? OpCodes.Brtrue : OpCodes.Brfalse;

        var targetLabel = new TargetLabel(processor.Body.Instructions.Count, node.Label); 
        targetLabels.Add(targetLabel);
        
        processor.Emit(opCode, Instruction.Create(OpCodes.Nop));
    }

    private void EmitReturn(ILProcessor processor, IR_ReturnStatment node) {
        if (node.Expression != null)
            EmitExpression(processor, node.Expression);

        processor.Emit(OpCodes.Ret);
    }

    private void EmitExpressionStatement(ILProcessor processor, IR_ExpressionStatement node) {
        EmitExpression(processor, node.Expression);

        if (node.Expression.Type != TypeSymbol.Void)
            processor.Emit(OpCodes.Pop);
    } 

    private void EmitExpression(ILProcessor processor, IR_Expression node) {
        switch (node.Kind) {
            case IR_NodeKind.LiteralExpression:
                EmitLiteral(processor, (IR_LiteralExpression)node);
                break;

            case IR_NodeKind.VariableExpression:
                EmitStoreVariable(processor, (IR_VariableExpression)node);
                break;

            case IR_NodeKind.AssignmentExpression:
                EmitAssignmentOperation(processor, (IR_AssignmentExpression)node);
                break;
            
            case IR_NodeKind.UnaryExpression:
                EmitUnaryOperation(processor, (IR_UnaryExpression)node);
                break;
            
            case IR_NodeKind.BinaryExpression:
                EmitBinaryOperation(processor, (IR_BinaryExpression)node);
                break;
            
            case IR_NodeKind.CallExpression:
                EmitCall(processor, (IR_CallExpression)node);
                break;
            
            case IR_NodeKind.CastExpression:
                EmitCasted(processor, (IR_CastExpression)node);
                break;
            
            default:
                throw new Exception($"Unexpected expression: {node.Kind}");
        }
    }

    private static void EmitLiteral(ILProcessor processor, IR_LiteralExpression node) {
        if (node.Type == TypeSymbol.Boolean) {
            var value = (bool)node.Value;
            var instruction = value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
            processor.Emit(instruction);
        }

        else if (node.Type == TypeSymbol.Int32) {
            var value = (int)node.Value;
            processor.Emit(OpCodes.Ldc_I4, value);
        }

        else if (node.Type == TypeSymbol.Double) {
            var value = (double)node.Value;
            processor.Emit(OpCodes.Ldc_R8, value);
        }

        else if (node.Type == TypeSymbol.String) {
            var value = (string)node.Value;
            processor.Emit(OpCodes.Ldstr, value);
        }

        else {
            throw new Exception($"Unexpected literal type: {node.Type}");
        }
    }

    private void EmitStoreVariable(ILProcessor processor, IR_VariableExpression node) {
        if (node.Variable is ParameterSymbol parameter) {
            processor.Emit(OpCodes.Ldarg, parameter.Index);
        } 
        else {
            var variable = locals[node.Variable];
            processor.Emit(OpCodes.Ldloc, variable);
        }
    }
    
    private void EmitAssignmentOperation(ILProcessor processor, IR_AssignmentExpression node) {
        var variable = locals[node.Variable];

        EmitExpression(processor, node.Expression);
        processor.Emit(OpCodes.Dup);
        processor.Emit(OpCodes.Stloc, variable);
    }
    
    private void EmitUnaryOperation(ILProcessor processor, IR_UnaryExpression node) {
        EmitExpression(processor, node.Operand);
        
        switch (node.Operator.Kind) {
            case UnaryOperator.Identity:
                // Do nothing
                break;

            case UnaryOperator.Negetion:
                processor.Emit(OpCodes.Neg);
                break;

            case UnaryOperator.BitwiseNot:
                processor.Emit(OpCodes.Not);
                break;

            case UnaryOperator.LogicalNot:
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;

            default:
                throw new Exception($"Unexpected unary operation: {node.Operator}");
        } 
    }

    private void EmitBinaryOperation(ILProcessor processor, IR_BinaryExpression node) {
        EmitExpression(processor, node.Left);
        EmitExpression(processor, node.Right);

        switch (node.Operator.Kind) {
            case BinaryOperator.Add:
                // string + string
                if (node.Left.Type == TypeSymbol.String && node.Left.Type == TypeSymbol.String)
                    processor.Emit(OpCodes.Call, resolvedMethods["Concat"]);

                processor.Emit(OpCodes.Add);
                break;
                
            case BinaryOperator.Substact:
                processor.Emit(OpCodes.Sub);
                break;

            case BinaryOperator.Multiply:
                processor.Emit(OpCodes.Mul);
                break;
            
            case BinaryOperator.Divide:
                processor.Emit(OpCodes.Div);
                break;
            
            case BinaryOperator.Remainder:
                processor.Emit(OpCodes.Rem);
                break;

            case BinaryOperator.Equals:
                // any == any
                // string == string
                if (
                    (node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any) ||
                    (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                ) {
                    processor.Emit(OpCodes.Call, resolvedMethods["ObjectEquals"]);
                    return;
                }

                processor.Emit(OpCodes.Ceq);
                break;
            
            case BinaryOperator.NotEquals:
                // any != any
                // string != string
                if (
                    (node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any) ||
                    (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                ) {
                    processor.Emit(OpCodes.Call, resolvedMethods["ObjectEquals"]);
                    processor.Emit(OpCodes.Ldc_I4_0);
                    processor.Emit(OpCodes.Ceq);
                    return;
                }

                processor.Emit(OpCodes.Ceq);
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;
            
            // Implement short circuit evaluation
            case BinaryOperator.LogicalAnd:
            case BinaryOperator.BitwiseAnd:
                processor.Emit(OpCodes.And);
                break;
            
            // Implement short circuit evaluation
            case BinaryOperator.LogicalOr:
            case BinaryOperator.BitwiseOr:
                processor.Emit(OpCodes.Or);
                break;
        
            case BinaryOperator.BitwiseXor:
                processor.Emit(OpCodes.Xor);
                break;
            
            case BinaryOperator.BitshiftLeft:
                processor.Emit(OpCodes.And);
                processor.Emit(OpCodes.Shl);
                break;
            
            case BinaryOperator.BitshiftRight:
                processor.Emit(OpCodes.And);
                processor.Emit(OpCodes.Shr);
                break;
            
            case BinaryOperator.GreaterThan:
                processor.Emit(OpCodes.Cgt);
                break;
            
            case BinaryOperator.GreaterThanEqualsTo:
                processor.Emit(OpCodes.Clt);
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;
            
            case BinaryOperator.LesserThan:
                processor.Emit(OpCodes.Clt);
                break;
            
            case BinaryOperator.LesserThanEqualsTo:
                processor.Emit(OpCodes.Cgt);
                processor.Emit(OpCodes.Ldc_I4_0);
                processor.Emit(OpCodes.Ceq);
                break;

            default:
                throw new Exception($"Unexpected binary operation: {node.Operator}");
        }
    }

    private void EmitCall(ILProcessor processor, IR_CallExpression node) {
        if (node.Function == BuiltinFunctions.RandInt) {
            if (randField is null)
                EmitRandomField();

            processor.Emit(OpCodes.Ldsfld, randField);
            
            foreach (var argument in node.Arguments)
                EmitExpression(processor, argument);

            processor.Emit(OpCodes.Callvirt, resolvedMethods["RandNext"]);
            return;
        }

        foreach (var argument in node.Arguments)
            EmitExpression(processor, argument);

        if (node.Function == BuiltinFunctions.Print)
            processor.Emit(OpCodes.Call, resolvedMethods["WriteLine"]);
        
        else if (node.Function == BuiltinFunctions.Input)
            processor.Emit(OpCodes.Call, resolvedMethods["ReadLine"]);

        else {
            var method = programMethods[node.Function];
            processor.Emit(OpCodes.Call, method);
        }
    }

    private void EmitRandomField() {
        randField = new FieldDefinition(
            "$rand",
            FieldAttributes.Static | FieldAttributes.Private,
            randType
        );
        mainProgram.Fields.Add(randField);

        var staticCtor = new MethodDefinition(
            ".cctor",
            MethodAttributes.Static | 
            MethodAttributes.Private | 
            MethodAttributes.SpecialName |
            MethodAttributes.RTSpecialName,
            knownTypes[TypeSymbol.Void]
        );
        mainProgram.Methods.Insert(0, staticCtor);

        var processor = staticCtor.Body.GetILProcessor();
        
        processor.Emit(OpCodes.Newobj, resolvedMethods["RandCtor"]);
        processor.Emit(OpCodes.Stsfld, randField);
        processor.Emit(OpCodes.Ret);
    }

    private void EmitCasted(ILProcessor processor, IR_CastExpression node) {
        EmitExpression(processor, node.Expression);

        var needsBoxing = node.Expression.Type == TypeSymbol.Boolean 
            || node.Expression.Type == TypeSymbol.Int32;

        if (needsBoxing)
            processor.Emit(OpCodes.Box, knownTypes[node.Expression.Type]);
        
        if (node.Type == TypeSymbol.Any) {
            // Do nothing
        }
        
        else if (node.Type == TypeSymbol.Boolean)
            processor.Emit(OpCodes.Call, resolvedMethods["ConvertToBoolean"]);

        else if (node.Type == TypeSymbol.Int32)
            processor.Emit(OpCodes.Call, resolvedMethods["ConvertToInt32"]);

        else if (node.Type == TypeSymbol.Double)
            processor.Emit(OpCodes.Call, resolvedMethods["ConvertToDouble"]);

        else if (node.Type == TypeSymbol.String)
            processor.Emit(OpCodes.Call, resolvedMethods["ConvertToString"]);

        else
            throw new Exception($"Unexpected cast from: {node.Expression.Type} to: {node.Type}");
    }

    public ImmutableArray<DiagnosticMessage> Diagonostics => [..diagnostics];


    private readonly Diagnostics diagnostics = [];
    private readonly List<AssemblyDefinition> referenceAssemblies = [];
    private readonly List<TargetLabel> targetLabels = []; 

    private readonly Dictionary<TypeSymbol, TypeReference> knownTypes = [];
    private readonly Dictionary<VariableSymbol, VariableDefinition> locals = [];
    private readonly Dictionary<FunctionSymbol, MethodDefinition> programMethods = [];
    
    private readonly Dictionary<string, MethodReference> resolvedMethods = [];
    private readonly Dictionary<LabelSymbol, int> labels = [];

    private AssemblyDefinition? programAssembly;
    private TypeDefinition? mainProgram;

    public TypeReference? randType;
    private FieldDefinition? randField;
}

internal sealed class TargetLabel(int index, LabelSymbol target) {
    public int Index { get; } = index;
    public LabelSymbol Target { get; } = target;
}