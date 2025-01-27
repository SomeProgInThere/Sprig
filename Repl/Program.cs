﻿using Rubics.Code.Syntax;
using Rubics.Code;

namespace Repl;

public static class Program {

    private static void Main() {
        var showTrees = false;
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
            var result = compilation.Evaluate();

            if (showTrees)
                PrettyPrint(syntaxTree.Root);

            if (!result.Diagnostics.Any()) {
                ColorPrint($"{result.Result}\n", ConsoleColor.Blue);
            }
            else {
                foreach (var diagnostic in result.Diagnostics)
                    ColorPrint($"{diagnostic}\n", ConsoleColor.Red);
            }
        }
    }

    static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true) {
        var marker = isLast ? "└──" : "├──";

        ColorPrint($"{indent}{marker}", ConsoleColor.DarkGray);
        ColorPrint($"{node.Kind}", ConsoleColor.White);

        if (node is Token token && token.Value != null) {
            ColorPrint($" ({token.Value})", ConsoleColor.Blue);
        }

        Console.WriteLine();
        indent += isLast ? "    " : "│   ";
        var lastChild = node.Children().LastOrDefault();

        foreach (var child in node.Children())
            PrettyPrint(child, indent, child == lastChild);
    }

    static void ColorPrint(string value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }
}
