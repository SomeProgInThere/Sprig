// using System.Collections.Immutable;

// using Sprig.Codegen.IRGeneration;
// using Sprig.Codegen.Symbols;

// namespace Sprig.Codegen;

// internal sealed class Evaluator {

//     public Evaluator(IRProgram program, Dictionary<VariableSymbol, object> variables) {
//         this.program = program;
//         globals = variables;

//         var current = program;
//         while (current != null) {
//             foreach (var function in current.Functions)
//                 functions.Add(function.Key, function.Value);

//             current = current.Previous;
//         }
//     }

//     public object? Evaluate() {
//         locals.Push([]);
//         if (program.MainFunction is null)
//             return null;

//         var body = functions[program.MainFunction];
//         return EvaluateStatement(body);
//     }

//     private object? EvaluateStatement(IRBlockStatement body) {
//         var labelTable = new Dictionary<LabelSymbol, int>();

//         for (var i = 0; i < body.Statements.Length; i++) {
//             if (body.Statements[i] is IRLabelStatement statement)
//                 labelTable.Add(statement.Label, i + 1);
//         }

//         var index = 0;
//         while (index < body.Statements.Length) {
//             var statement = body.Statements[index];
//             switch (statement.Kind) {

//                 case IRNodeKind.VariableDeclaration:
//                     EvaluateVariableDeclaration((IRVariableDeclaration)statement);
//                     index++;
//                     break;

//                 case IRNodeKind.ExpressionStatement:
//                     EvaluateExpressionStatement((IRExpressionStatement)statement);
//                     index++;
//                     break;

//                 case IRNodeKind.GotoStatement:
//                     var gotoStatement = (IRGotoStatement)statement;
//                     index = labelTable[gotoStatement.Label];
//                     break;

//                 case IRNodeKind.ConditionalGotoStatement:
//                     var conditionalGotoStatement = (IRConditionalGotoStatement)statement;
//                     var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);

//                     if (condition && conditionalGotoStatement.Jump 
//                         || !condition && !conditionalGotoStatement.Jump)
                        
//                         index = labelTable[conditionalGotoStatement.Label];
//                     else
//                         index++;
//                     break;

//                 case IRNodeKind.LabelStatement:
//                     index++;
//                     break;

//                 case IRNodeKind.ReturnStatement:
//                     var returnStatment = (IRReturnStatment)statement;
//                     lastValue = returnStatment.Expression is null
//                         ? null
//                         : EvaluateExpression(returnStatment.Expression);

//                     return lastValue;

//                 default:
//                     throw new Exception($"Undefined statement: {statement.Kind}");
//             }
//         }

//         return lastValue;
//     }

//     private void EvaluateVariableDeclaration(IRVariableDeclaration node) {
//         var value = EvaluateExpression(node.Initializer);
//         lastValue = value;
//         AssignValue(node.Variable, value);
//     }
    
//     private void EvaluateExpressionStatement(IRExpressionStatement node) => lastValue = EvaluateExpression(node.Expression);
    
//     private object EvaluateExpression(IRExpression? node) {
//         return node?.Kind switch {
//             IRNodeKind.LiteralExpression     => EvaluateLiteralExpression((IRLiteralExpression)node),
//             IRNodeKind.VariableExpression    => EvaluateVariableExpression((IRVariableExpression)node),
//             IRNodeKind.AssignmentExpression  => EvaluateAssignmentExpression((IRAssignmentExpression)node),
//             IRNodeKind.UnaryExpression       => EvaluateUnaryExpression((IRUnaryExpression)node),
//             IRNodeKind.BinaryExpression      => EvaluateBinaryExpression((IRBinaryExpression)node),
//             IRNodeKind.RangeExpression       => EvaluateRangeExpression((IRRangeExpression)node),
//             IRNodeKind.CallExpression        => EvaluateCallExpression((IRCallExpression)node),
//             IRNodeKind.CastExpression        => EvaluateCastExpression((IRCastExpression)node),
            
//             _ => throw new Exception($"Undefined node: {node?.Kind}"),
//         };
//     }

//     private static object EvaluateLiteralExpression(IRLiteralExpression literal) => literal.Value;
    
//     private object EvaluateVariableExpression(IRVariableExpression node) {
//         var variable = node.Variable 
//             ?? throw new Exception($"Node variable not initialized");
        
//         if (variable.Scope == VariableScope.Global)
//             return globals[variable];
                
//         else {
//             var localVariables = locals.Peek();
//             return localVariables[variable];
//         }
//     }

//     private object EvaluateAssignmentExpression(IRAssignmentExpression node) {
//         var value = EvaluateExpression(node.Expression);
//         AssignValue(node.Variable, value);
//         return value;
//     }

//     private object EvaluateUnaryExpression(IRUnaryExpression node) {
//         var operand = EvaluateExpression(node.Operand);

//         if (node.Type == TypeSymbol.Float) {
//             return node.Operator.Kind switch {
//                 UnaryOperator.Identity => (float)operand,
//                 UnaryOperator.Negetion => -(float)operand,
//                 _ => throw new Exception($"Unexpected Unary operator: {node.Operator}"),
//             };
//         }

//         return node.Operator.Kind switch {
//             UnaryOperator.Identity   => (int)operand,
//             UnaryOperator.Negetion   => -(int)operand,
//             UnaryOperator.BitwiseNot => ~(int)operand,
//             UnaryOperator.LogicalNot => !(bool)operand,
            
//             _ => throw new Exception($"Unexpected Unary operator: {node.Operator}"),
//         };
//     }

//     private object EvaluateBinaryExpression(IRBinaryExpression node) {
//         var left = EvaluateExpression(node.Left);
//         var right = EvaluateExpression(node.Right);

//         switch (node.Operator.Kind) {
//             case BinaryOperator.Add: 
//                 if (node.Type == TypeSymbol.String)
//                     return (string)left + (string)right;

//                 if (node.Type == TypeSymbol.Float)
//                     return (float)left + (float)right;

//                 return (int)left + (int)right;
            
//             case BinaryOperator.Substact:            
//                 if (node.Type == TypeSymbol.Float)
//                     return (float)left - (float)right;

//                 return (int)left - (int)right;
            
//             case BinaryOperator.Multiply:
//                 if (node.Type == TypeSymbol.String) {
//                     var str = (string)left;
//                     for (var i = 1; i < (int)right; i++)
//                         str += (string)left;

//                     return str;
//                 }

//                 if (node.Type == TypeSymbol.Float)
//                     return (float)left + (float)right;

//                 return (int)left * (int)right;

//             case BinaryOperator.Divide:                 
//                 if (node.Type == TypeSymbol.Float)
//                     return (float)left / (float)right;

//                 return (int)left / (int)right;
            
//             case BinaryOperator.Modulus:            
//                 if (node.Type == TypeSymbol.Float)
//                     return (float)left % (float)right;
 
//                 return (int)left % (int)right;
            
//             case BinaryOperator.Remainder: 
//                 return Math.DivRem((int)left, (int)right).Remainder;
            
//             case BinaryOperator.RaisePower:             
//                 if (node.Type == TypeSymbol.Float)
//                     return (float)Math.Pow((float)left, (float)right);

//                 return (int)Math.Pow((int)left, (int)right);
             
//             case BinaryOperator.LogicalAnd: 
//                 return (bool)left && (bool)right;
            
//             case BinaryOperator.LogicalOr: 
//                 return (bool)left || (bool)right;

//             case BinaryOperator.BitwiseAnd: 
//                 return (int)left & (int)right;
            
//             case BinaryOperator.BitwiseOr: 
//                 return (int)left | (int)right;
            
//             case BinaryOperator.BitwiseXor: 
//                 return (int)left ^ (int)right;
            
//             case BinaryOperator.BitshiftLeft: 
//                 return (int)left >> (int)right;
            
//             case BinaryOperator.BitshiftRight: 
//                 return (int)left << (int)right;

//             case BinaryOperator.GreaterThan:
//              if (node.Type == TypeSymbol.Float)
//                     return (float)left > (float)right;

//                 return (int)left > (int)right;
            
//             case BinaryOperator.GreaterThanEqualsTo:
//              if (node.Type == TypeSymbol.Float)
//                     return (float)left >= (float)right;

//                 return (int)left >= (int)right;
            
//             case BinaryOperator.LesserThan:
//              if (node.Type == TypeSymbol.Float)
//                     return (float)left < (float)right;

//                 return (int)left < (int)right;
            
//             case BinaryOperator.LesserThanEqualsTo:
//              if (node.Type == TypeSymbol.Float)
//                     return (float)left <= (float)right;

//                 return (int)left <= (int)right;

//             case BinaryOperator.Equals: 
//                 return Equals(left, right);
            
//             case BinaryOperator.NotEquals: 
//                 return !Equals(left, right);

//             default: 
//                 throw new Exception($"Unexpected Binary operator: {node.Operator}");
//         };
//     }

//     private object EvaluateRangeExpression(IRRangeExpression node) {
//         var lower = EvaluateExpression(node.Lower);
//         var upper = EvaluateExpression(node.Upper);
//         return (lower, upper);
//     }

//     private object EvaluateCallExpression(IRCallExpression node) {
//         if (node.Function == BuiltinFunctions.Input) {
//             return Console.ReadLine() ?? "";
//         }

//         else if (node.Function == BuiltinFunctions.Print) {
//             var message = (string)EvaluateExpression(node.Arguments[0]);
//             Console.WriteLine(message);
//             return "";
//         }

//         else if (node.Function == BuiltinFunctions.Random) {
//             var min = (int)EvaluateExpression(node.Arguments[0]);
//             var max = (int)EvaluateExpression(node.Arguments[1]);
            
//             random ??= new Random();
//             return random.Next(min, max);
//         }

//         else {
//             var stackframe = new Dictionary<VariableSymbol, object>();
            
//             for (var i = 0; i < node.Arguments.Length; i++) {
//                 var parameter = node.Function.Parameters[i];
//                 var value = EvaluateExpression(node.Arguments[i]);
//                 stackframe.Add(parameter, value);
//             }

//             locals.Push(stackframe);
//             var statement = functions[node.Function];
//             var result = EvaluateStatement(statement);
//             locals.Pop();

//             return result ?? "";
//         }
//     }

//     private object EvaluateCastExpression(IRCastExpression node) {
//         var value = EvaluateExpression(node.Expression);
        
//         if (node.Type == TypeSymbol.Any)
//             return value;

//         if (node.Type == TypeSymbol.Bool)
//             return Convert.ToBoolean(value);
        
//         else if (node.Type == TypeSymbol.Int)
//             return Convert.ToInt32(value);
        
//         else if (node.Type == TypeSymbol.String)
//             return Convert.ToString(value) ?? "";

//         else if (node.Type == TypeSymbol.Float)
//             return Convert.ToSingle(value);
//         else
//             throw new Exception($"Undefined type: {node.Type}");
//     }

//     private void AssignValue(VariableSymbol variable, object value) {
//         if (variable.Scope == VariableScope.Global)
//             globals[variable] = value;

//         else {
//             var localVariables = locals.Peek();
//             localVariables[variable] = value;
//         }
//     }

//     private Random? random;
//     private object? lastValue;

//     private readonly IRProgram program;
//     private readonly Stack<Dictionary<VariableSymbol, object>> locals = [];
//     private readonly Dictionary<FunctionSymbol, IRBlockStatement> functions = [];
//     private readonly Dictionary<VariableSymbol, object> globals;
// }

// public sealed class EvaluationResult(ImmutableArray<DiagnosticMessage> diagnostics, object? result = null) {
//     public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
//     public object? Result { get; } = result;
// }