using System.CommandLine;

using Sprig.Codegen;
using Sprig.Codegen.Syntax;
using Sprig.IO;

class Program {

    static async Task<int> Main(string[] args) {
        var rootCommand = new RootCommand("Twig: CLI for sprig language");
        
        var buildCommand = new Command("build", "Compiles the given source");
        var sourcePathsArgument = new Argument<List<string>>(
            name: "source-paths",
            description: "Paths to the source files"
        );
        
        var referencePathsOption = new Option<string>(
            name: "--reference",
            description: "Path of the assembly to reference"
        );
        
        referencePathsOption.AddAlias("-r");
        referencePathsOption.AllowMultipleArgumentsPerToken = true;
        
        var moduleNameOption = new Option<string>(
            name: "--module",
            description: "Name of the module"
        );

        moduleNameOption.AddAlias("-m");
        
        var outputOption = new Option<string>(
            name: "--output",
            description: "Output path of the assembly to create"
        );

        outputOption.AddAlias("-o");
        
        var dumpPathOption = new Option<string>(
            name: "--dump",
            description: "Path of the extra outputs of program"
        );
        
        dumpPathOption.AddAlias("-d");

        buildCommand.Add(sourcePathsArgument);
        
        buildCommand.Add(referencePathsOption);
        buildCommand.Add(moduleNameOption);
        buildCommand.Add(outputOption);
        buildCommand.Add(dumpPathOption);

        rootCommand.Add(buildCommand);

        buildCommand.SetHandler((sourcePaths, reference, module, output, dumpPath) => {                
                var referencePaths = new List<string>();
                if (reference != null)
                    referencePaths.Add(reference);
                
                if (sourcePaths.Count == 0) {
                    PrintError("no source paths provided");
                    return;
                }

                var outputPath = output ?? Path.ChangeExtension(sourcePaths[0], ".exe");
                var moduleName = module ?? Path.GetFileNameWithoutExtension(outputPath);
                
                var syntaxTrees = new List<SyntaxTree>();
                var hasErrors = false;
                
                foreach (var path in sourcePaths) {
                    if (!File.Exists(path)) {
                        PrintError($"source file '{path}' does not exist");
                        hasErrors = true;
                        continue;
                    }
                
                    var syntaxTree = SyntaxTree.Load(path);
                    syntaxTrees.Add(syntaxTree);
                }

                if (referencePaths.Count != 0) {
                    foreach (var path in referencePaths) {
                        if (!File.Exists(path)) {
                            PrintError($"reference file '{path}' does not exist");
                            hasErrors = true;
                            continue;
                        }                
                    }
                }

                if (hasErrors)
                    return;

                var compilation = Compilation.Create([..syntaxTrees]);
                var diagnostics = compilation.Emit(moduleName, [..referencePaths], outputPath, dumpPath);

                if (diagnostics.Any()) {
                    Console.Error.WriteDiagnostics(diagnostics);
                    return;
                }
            },
            sourcePathsArgument,
            referencePathsOption,
            moduleNameOption,
            outputOption,
            dumpPathOption
        );

      //return await rootCommand.InvokeAsync(args);
      return await rootCommand.InvokeAsync([
        "build",
        "C:/Users/calla/Dev/Sprig/samples/hello.sg",
        "-r", 
        "C:/Windows/Microsoft.NET/Framework64/v4.0.30319/mscorlib.dll",
        "-d", 
        "C:/Users/calla/Dev/Sprig/samples/hello.g.sg"
        ]);
    }

    private static void PrintError(string message) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Error.Write("error: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Error.Write(message);
        Console.ResetColor();
        Console.WriteLine();
    }
}