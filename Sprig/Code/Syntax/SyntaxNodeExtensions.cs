namespace Sprig.Code.Syntax;

internal sealed class SyntaxNodeExtensions {
    public static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool last = true) {
        var consoleOut = writer == Console.Out;
        var marker = last ? "└──" : "├──";

        Console.ForegroundColor = ConsoleColor.DarkGray;
        writer.Write(indent);

        if (consoleOut)
            Console.ForegroundColor = ConsoleColor.DarkGray;
            writer.Write(marker);
            Console.ForegroundColor = node is Token ? ConsoleColor.Blue : ConsoleColor.DarkMagenta;

        writer.Write(node.Kind);

        if (node is Token token && token.Value != null)
            writer.Write($" ({token.Value})");

        if (consoleOut)
            Console.ResetColor();

        writer.WriteLine();
        indent += last ? "    " : "│   ";

        var lastChild = node.Children().LastOrDefault();
        foreach (var child in node.Children())
            PrettyPrint(writer, child, indent, child == lastChild);
    }
}