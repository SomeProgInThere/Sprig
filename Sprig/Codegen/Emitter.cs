using System.Collections.Immutable;
using Mono.Cecil;
using Mono.Cecil.Cil;

using Sprig.Codegen.IRGeneration;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

internal sealed class Emitter {

    public ImmutableArray<DiagnosticMessage> Emit(
        IRProgram program, string moduleName, string[] references, string outputPath) {
        
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        foreach (var reference in references) {
            try {
                var assembly = AssemblyDefinition.ReadAssembly(reference);
                assemblies.Add(assembly);
            } 
            catch (BadImageFormatException) {
                diagnostics.ReportInvalidReferenceLinking(reference);
            }
        }

        var builtinTypes = new List<(TypeSymbol type, string metadataName)>() {
            (TypeSymbol.Any,    "System.Object"),
            (TypeSymbol.Void,   "System.Void"),
            (TypeSymbol.Int,    "System.Int32"),
            (TypeSymbol.Float,  "System.Single"),
            (TypeSymbol.String, "System.String"),
        };

        var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
        var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
        
        TypeReference? ResolveType(string? typeName, string metadataName) {
            var foundTypes = assemblies
                .SelectMany(assembly => assembly.Modules)
                .SelectMany(module => module.Types)
                .Where(type => type.FullName == metadataName)
                .ToArray();

            if (foundTypes.Length == 1) {
                var typeReference = assemblyDefinition.MainModule.ImportReference(foundTypes[0]);
                return typeReference;
            }

            else if (foundTypes.Length == 0)
                diagnostics.ReportRequiredTypeNotFound(typeName, metadataName);
            
            else
                diagnostics.ReportRequiredTypeAmbiguous(typeName, metadataName, foundTypes);

            return null;
        }

        MethodReference? ResolveMethod(string typeName, string methodName, string[] parameterTypeNames) {
            var foundTypes = assemblies
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

                    var methodReference = assemblyDefinition.MainModule.ImportReference(method);
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
            var typeReference = ResolveType(typeSymbol.Name, metadataName);
            knownTypes.Add(typeSymbol, typeReference);
        }

        var consoleType = assemblies
                .SelectMany(assembly => assembly.Modules)
                .SelectMany(module => module.Types)
                .Where(type => type.FullName == "System.Console")
                .ToArray();

        var consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", ["System.String"]);

        if (diagnostics.Any())
            return [..diagnostics];
        
        /*
            static sealed class Program {
                void Main() {
                    System.Console.WriteLine("Hello World!");
                }
            }
        */

        var objectType = knownTypes[TypeSymbol.Any];
        var typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
        assemblyDefinition.MainModule.Types.Add(typeDefinition);

        var voidType = knownTypes[TypeSymbol.Void];
        var mainMethod = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, voidType);
        typeDefinition.Methods.Add(mainMethod);

        var ilProcessor = mainMethod.Body.GetILProcessor();
        
        ilProcessor.Emit(OpCodes.Ldstr, "Hello from Sprig!");
        ilProcessor.Emit(OpCodes.Call, consoleWriteLineReference);
        ilProcessor.Emit(OpCodes.Ret);

        assemblyDefinition.EntryPoint = mainMethod;
        assemblyDefinition.Write(outputPath);

        return [..diagnostics];
    }

    private readonly Dictionary<TypeSymbol, TypeReference> knownTypes = [];
    private readonly List<AssemblyDefinition> assemblies = [];
    private readonly Diagnostics diagnostics = [];
}