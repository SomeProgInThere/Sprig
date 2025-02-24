
using Sprig.Code.Syntax;
using Sprig.Code.Source;
using Sprig.Code;
using System.Text;
using System.Runtime.InteropServices;

namespace Sprig.CLI;

public static class Program {

    private static void Main() {
        
        var showTrees = false;
        var showProgram = false;
        var variables = new Dictionary<VariableSymbol, object>();
        var sourceBuilder = new StringBuilder();

        Compilation? previous = null;

        ColorPrint($"{startupMessage}\n\n", ConsoleColor.DarkGray);

        while (true) {
            if (sourceBuilder.Length == 0)
                ColorPrint("» ", ConsoleColor.Cyan);
            else
                ColorPrint("  ", ConsoleColor.DarkGray);

            var input = Console.ReadLine();

            if (sourceBuilder.Length == 0) {
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input == "!exit") {
                    ColorPrint("Terminated\n\n", ConsoleColor.DarkGray);
                    return;
                }

                if (input == "!clear") { 
                    Console.Clear(); 
                    continue; 
                }

                if (input == "!parse") {
                    showTrees = !showTrees;
                    ColorPrint($"Info: show parse tree set to {showTrees}\n\n", ConsoleColor.DarkGray);
                    continue;
                }

                if (input == "!program") {
                    showProgram = !showProgram;
                    ColorPrint($"Info: show program tree set to {showProgram}\n\n", ConsoleColor.DarkGray);
                    continue;
                }

                if (input == "!reset") {
                    previous = null;
                    ColorPrint("Info: environment reset done\n\n", ConsoleColor.DarkGray);
                    continue;
                }

                if (input == "!help") {
                    DisplayCommands();
                    continue;
                }
            }
            
            sourceBuilder.AppendLine(input);
            var source = sourceBuilder.ToString();
            var syntaxTree = SyntaxTree.Parse(source);

            if (!string.IsNullOrWhiteSpace(input) && syntaxTree.Diagnostics.Any())
                continue;

            var compilation = previous is null ? new Compilation(syntaxTree) : previous.ContinueWith(syntaxTree);
            var result = compilation.Evaluate(variables);

            if (showTrees)
                syntaxTree.Root.WriteTo(Console.Out);

            if (showProgram)
                compilation.EmitTree(Console.Out);

            if (!result.Diagnostics.Any()) {
                ColorPrint($"{result.Result}\n", ConsoleColor.Blue);
                previous = compilation;
            }
            else {
                var sourceText = syntaxTree.SourceText;
                foreach (var diagnostic in result.Diagnostics) {

                    var lineIndex = sourceText.GetLineIndex(diagnostic.Span.Start);
                    var line = sourceText.Lines[lineIndex];
                    
                    var column = diagnostic.Span.Start - line.Start + 1;
                    var lineNumber = lineIndex + 1;

                    Console.WriteLine();
                    ColorPrint($"Error (Ln {lineNumber}, Col {column}): ", ConsoleColor.Red);
                    ColorPrint($"{diagnostic}\n", ConsoleColor.Gray);

                    var prefixSpan = TextSpan.CreateFromBounds(line.Start, diagnostic.Span.Start);
                    var suffixSpan = TextSpan.CreateFromBounds(diagnostic.Span.End, line.End);

                    var prefix = sourceText.ToString(prefixSpan).Trim('\t');
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

    static void DisplayCommands() {
        var commands = new List<(string, string)> {
            ("help", "show list of commands"),
            ("exit", "terminate the session"),
            ("clear", "clears the output console"),
            ("parse", "show internal parse trees for source"),
            ("program", "show internal bound tree for source")
        };

        foreach (var c in commands) {
            ColorPrint($"\n\t!{c.Item1}:", ConsoleColor.Gray);
            ColorPrint($"\t{c.Item2}", ConsoleColor.DarkGray);
        }

        Console.WriteLine("\n");
    }

    static void ColorPrint(string value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }

    static readonly string startupMessage = $"""
        Sprig
        {RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture}) 
        Type "!help" for list of commands
    """;
}