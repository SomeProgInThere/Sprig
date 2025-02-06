
using Rubics.Code.Syntax;
using Rubics.Code.Source;
using Rubics.Code;
using System.Text;
using System.Runtime.InteropServices;

namespace Rubics.Repl;

public static class Program {

    private static void Main() {
        var showTrees = false;
        var variables = new Dictionary<VariableSymbol, object>();
        var sourceBuilder = new StringBuilder();

        ColorPrint($"{startupMessage}\n\n", ConsoleColor.DarkBlue);

        while (true) {
            if (sourceBuilder.Length == 0)
                ColorPrint(">> ", ConsoleColor.Cyan);
            else
                ColorPrint(" . ", ConsoleColor.DarkGray);

            var input = Console.ReadLine();

            if (sourceBuilder.Length == 0) {
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input == "!exit") {
                    ColorPrint("Terminated.\n\n", ConsoleColor.DarkGray);
                    return;
                }

                if (input == "!clear") { 
                    Console.Clear(); 
                    continue; 
                }

                if (input == "!tree") {
                    showTrees = !showTrees;
                    ColorPrint($"INFO: showTrees set to: {showTrees}\n\n", ConsoleColor.DarkGray);
                    continue;
                }
            }
            
            sourceBuilder.AppendLine(input);
            var source = sourceBuilder.ToString();
            var syntaxTree = SyntaxTree.Parse(source);

            if (!string.IsNullOrWhiteSpace(input) && syntaxTree.Diagnostics.Any())
                continue;

            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(variables);

            if (showTrees)
                syntaxTree.Root.WriteTo(Console.Out);

            if (!result.Diagnostics.Any()) {
                ColorPrint($"{result.Result}\n", ConsoleColor.White);
            }
            else {
                var sourceText = syntaxTree.SourceText;
                foreach (var diagnostic in result.Diagnostics) {

                    var lineIndex = sourceText.GetLineIndex(diagnostic.Span.Start);
                    var line = sourceText.Lines[lineIndex];
                    
                    var column = diagnostic.Span.Start - line.Start + 1;
                    var lineNumber = lineIndex + 1;

                    Console.WriteLine();
                    ColorPrint($"ERROR (Ln {lineNumber}, Col {column}): ", ConsoleColor.DarkRed);
                    ColorPrint($"{diagnostic}\n", ConsoleColor.Gray);

                    var prefixSpan = TextSpan.CreateFromBounds(line.Start, diagnostic.Span.Start);
                    var suffixSpan = TextSpan.CreateFromBounds(diagnostic.Span.End, line.End);

                    var prefix = sourceText.ToString(prefixSpan);
                    var error = sourceText.ToString(diagnostic.Span);
                    var suffix = sourceText.ToString(suffixSpan);

                    Console.Write($"\t{prefix}");
                    ColorPrint(error, ConsoleColor.Red);
                    Console.Write($"{suffix}\n");
                }
            }

            sourceBuilder.Clear();
        }
    }

    static void ColorPrint(string value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }

    static readonly string startupMessage = $"""
        Rubics v0alpha
        {RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture}) 
        Commands: "!tree", "!clear", "!exit"
    """;
}