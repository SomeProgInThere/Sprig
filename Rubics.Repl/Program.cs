using Rubics.Code.Syntax;
using Rubics.Code;

namespace Repl;

public static class Program {

    private static void Main() {
        var showTrees = false;
        var variables = new Dictionary<VariableSymbol, object>();

        while (true) {

            ColorPrint("> ", ConsoleColor.Yellow);
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            if (line == "!exit") 
                return;
            
            if (line == "!clear") { 
                Console.Clear(); 
                continue; 
            }

            if (line == "!showTrees") {
                showTrees = !showTrees;
                ColorPrint($"INFO: showTrees set to: {showTrees}\n", ConsoleColor.DarkGray);
                continue;
            }

            var syntaxTree = SyntaxTree.Parse(line);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(variables);

            if (showTrees)
                syntaxTree.Root.WriteTo(Console.Out);

            if (!result.Diagnostics.Any()) {
                ColorPrint($"{result.Result}\n", ConsoleColor.Blue);
            }
            else {
                foreach (var diagnostic in result.Diagnostics) {
                    Console.WriteLine();
                    ColorPrint($"{diagnostic}\n", ConsoleColor.Red);

                    var prefix = line[..diagnostic.Span.Start];
                    var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                    var suffix = line[diagnostic.Span.End..];

                    Console.Write($"\t{prefix}");
                    ColorPrint(error, ConsoleColor.Red);

                    Console.Write($"{suffix}\n");
                }

                Console.WriteLine();
            }
        }
    }

    static void ColorPrint(string value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }
}
