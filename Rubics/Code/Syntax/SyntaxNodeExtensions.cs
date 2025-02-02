
namespace Rubics.Code.Syntax;

internal sealed class SyntaxNodeExtensions {
    public static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool last = true) {
        var marker = last ? "└──" : "├──";
        writer.Write($"{indent}{marker}{node.Kind}");

        if (node is Token token && token.Value != null)
            writer.Write($" ({token.Value})");

        Console.WriteLine();
        indent += last ? "    " : "│   ";

        var lastChild = node.Children().LastOrDefault();
        foreach (var child in node.Children())
            PrettyPrint(writer, child, indent, child == lastChild);
    }
}