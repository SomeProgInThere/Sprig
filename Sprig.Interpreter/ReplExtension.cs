using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using Sprig.Codegen.Text;
using Sprig.Codegen.Syntax;

namespace Sprig.Interpreter;

internal sealed class ReplExtensions {

    internal static bool IsCompleteSource(string source) {
        if (string.IsNullOrEmpty(source))
            return true;

        // Forces completion due to two consecutive blank lines
        var forceComplete = source
                                .Split(Environment.NewLine)
                                .Reverse()
                                .TakeWhile(string.IsNullOrEmpty)
                                .Take(2)
                                .Count() == 2;

        if (forceComplete)
            return true;

        var syntaxTree = SyntaxTree.Parse(source);
        if (syntaxTree.Root.Members.Last().LastToken().IsMissing)
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
            return;
        }

        var lineIndex = view.CurrentLine;
        var line = document[lineIndex];
        var before = line[..(start - 1)];
        var after = line[start..];

        document[lineIndex] = before + after;
        view.CurrentChar--;        
    }

    internal static void HandleEscape(ObservableCollection<string> document, SourceView view) {
        document.Clear();
        document.Add(string.Empty);
        view.CurrentLine = 0;
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