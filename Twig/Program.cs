using System.CommandLine;

using Sprig.Codegen;
using Sprig.Codegen.Syntax;
using Sprig.IO;

class Program {

    static async Task<int> Main(string[] args) {
        var rootCommand = new RootCommand("Twig: CLI for sprig-lang");
        
        var buildCommand = new Command("build", "Builds the given sources");
        var sourcePathsArgument = new Argument<List<string>>(
            name: "source-paths",
            description: "Paths to the source files"
        );
        
        var referencePathsOption = new Option<string>(
            name: "--reference",
            description: "Paths of the .NET assemblies to reference",
            // Limited to windows
            getDefaultValue: () => "C:/Windows/Microsoft.NET/Framework64/v4.0.30319/mscorlib.dll"
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
            description: "Output path of the program"
        );

        outputOption.AddAlias("-o");
        
        var dumpOption = new Option<string[]>(
            name: "--dump",
            description: "Outputs internal representations of the program"
        )
        .FromAmong(
            "controlflow",
            "lowered",
            "tokens",
            "symbols"
        );
        
        dumpOption.AddAlias("-d");
        dumpOption.AllowMultipleArgumentsPerToken = true;

        buildCommand.Add(sourcePathsArgument);
        
        buildCommand.Add(referencePathsOption);
        buildCommand.Add(moduleNameOption);
        buildCommand.Add(outputOption);
        buildCommand.Add(dumpOption);

        rootCommand.Add(buildCommand);

        buildCommand.SetHandler((sourcePaths, reference, module, output, dumpOptions) => {                
                var referencePaths = new List<string>();
                if (reference != null)
                    referencePaths.Add(reference);
                
                if (sourcePaths.Count == 0) {
                    PrintError("No source paths provided");
                    return;
                }

                var outputPath = output ?? Path.ChangeExtension(sourcePaths[0], ".exe");
                var moduleName = module ?? Path.GetFileNameWithoutExtension(outputPath);
                
                var syntaxTrees = new List<SyntaxTree>();
                var hasErrors = false;
                
                foreach (var path in sourcePaths) {
                    if (!File.Exists(path)) {
                        PrintError($"Source file '{path}' does not exist");
                        hasErrors = true;
                        continue;
                    }
                
                    var syntaxTree = SyntaxTree.Load(path);
                    syntaxTrees.Add(syntaxTree);
                }

                if (referencePaths.Count != 0) {
                    foreach (var path in referencePaths) {
                        if (!File.Exists(path)) {
                            PrintError($"Reference file '{path}' does not exist");
                            hasErrors = true;
                            continue;
                        }                
                    }
                }

                if (hasErrors)
                    return;

                if (dumpOptions.Length != 0) {
                    if (dumpOptions.Select(option => option == "tokens").First()) {
                        foreach (var tree in syntaxTrees) {
                            var dumpPath = Path.ChangeExtension(
                                Path.Join(Path.GetDirectoryName(outputPath), tree.Source.FileName), 
                                ".tokens.txt"
                            );
                            using var writer = new StreamWriter(dumpPath);
                            writer.WriteLine($"<generated parse tree from source file: {Path.ChangeExtension(outputPath, ".sg")}>");
                            tree.Root.WriteTo(writer);
                        }
                    }
                }

                var compilation = Compilation.Create([..syntaxTrees]);
                var diagnostics = compilation.Emit(moduleName, [..referencePaths], outputPath, dumpOptions);
                
                if (diagnostics.Any()) {
                    Console.Error.WriteDiagnostics(diagnostics);
                    return;
                }
            },
            sourcePathsArgument,
            referencePathsOption,
            moduleNameOption,
            outputOption,
            dumpOption
        );

        return await rootCommand.InvokeAsync(args);
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