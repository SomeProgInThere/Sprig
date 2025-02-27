using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using Sprig.Code;
using Sprig.Code.Source;
using Sprig.Code.Syntax;

namespace Sprig.CLI;

internal sealed class ReplExtensions {

    internal static void DisplayDiagnostics(SyntaxTree syntaxTree, EvaluationResult result) {
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

    internal static bool IsCompleteSource(string source) {
        if (string.IsNullOrEmpty(source))
            return true;

        var syntaxTree = SyntaxTree.Parse(source);
        if (syntaxTree.Root.Statement.LastToken().IsMissing)
            return false;

        return true;
    }

    internal static void HandleTab(ObservableCollection<string> document, SourceView view) {
        const int tabWidth = 4;
        var start = view.CurrentChar;
        var remainingSpaces = tabWidth - start % tabWidth;
        var line = document[view.CurrentLine];

        document[view.CurrentLine] = line.Insert(start, new string(' ', remainingSpaces));
        view.CurrentChar += remainingSpaces;
    }

    internal static void HandleBackspace(ObservableCollection<string> document, SourceView view) {
        var start = view.CurrentChar;
        if (start == 0) {
            if (view.CurrentLine == 0)
                return;
            
            var currentLine = document[view.CurrentLine];
            var previousLine = document[view.CurrentLine - 1];
            
            document.RemoveAt(view.CurrentLine);
            view.CurrentLine--;
            document[view.CurrentLine] = previousLine + currentLine;
            view.CurrentChar = previousLine.Length;
        }

        var lineIndex = view.CurrentLine;
        var line = document[lineIndex];
        var before = line[..(start - 1)];
        var after = line[start..];

        document[lineIndex] = before + after;
        view.CurrentChar--;        
    }

    internal static void HandleDelete(ObservableCollection<string> document, SourceView view) {
        var lineIndex = view.CurrentLine;
        var line = document[lineIndex];
        var start = view.CurrentChar;
        if (start >= line.Length)
            return;

        var before = line[..start];
        var after = line[(start + 1)..];

        document[lineIndex] = before + after;
    }

    internal static void HandleTyping(ObservableCollection<string> document, SourceView view, string keyChar) {
        var lineIndex = view.CurrentLine;
        var start = view.CurrentChar;

        document[lineIndex] = document[lineIndex].Insert(start, keyChar);
        view.CurrentChar += keyChar.Length;
    }

    internal static void DisplayCommands() {
        var commands = new List<(string, string)> {
            ("help", "show list of commands"),
            ("exit", "terminate the session"),
            ("clear", "clears the output console"),
            ("parse", "show internal parse trees for source"),
            ("binding", "show internal bound tree for source")
        };

        foreach (var command in commands) {
            ColorPrint($"\n!{command.Item1}:", ConsoleColor.Gray);
            ColorPrint($"\t{command.Item2}", ConsoleColor.DarkGray);
        }

        Console.WriteLine("\n");
    }

    internal static void ColorPrint(string value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }
    
    internal static void InsertLine(ObservableCollection<string> document, SourceView view) {
        var remainder = document[view.CurrentLine][view.CurrentChar..];
        document[view.CurrentLine] = document[view.CurrentLine][..view.CurrentChar];

        var lineIndex = view.CurrentLine + 1;
        document.Insert(lineIndex, remainder);
        view.CurrentChar = 0;
        view.CurrentLine = lineIndex;
    }

    internal static readonly string StartupMessage = $"""
    Sprig [{RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}]
    Type "!help" for list of commands
    """;
}