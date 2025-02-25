

namespace Sprig.Code.Binding;

internal abstract class BoundNodeExtensions {
    public static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool last = true) {
        var consoleOut = writer == Console.Out;
        var marker = last ? "└──" : "├──";

        if (consoleOut)
            Console.ForegroundColor = ConsoleColor.DarkGray;

        writer.Write(indent);
        writer.Write(marker);

        if (consoleOut) {
            Console.ForegroundColor = node switch {
                BoundExpression => ConsoleColor.DarkBlue,
                BoundStatement => ConsoleColor.DarkCyan,
                _ => ConsoleColor.Yellow,
            };
        }

        var expressionLiteral = node switch {
            BoundBinaryExpression binary => binary.Operator.Kind.ToString() + "Expression",
            BoundUnaryExpression unary => unary.Operator.Kind.ToString() + "Expression",
            _ => node.Kind.ToString(),
        };

        writer.Write(expressionLiteral);
        WriteProperties(writer, node, consoleOut);

        if (consoleOut)
            Console.ResetColor();

        writer.WriteLine();
        indent += last ? "    " : "│   ";

        var lastChild = node.Children().LastOrDefault();
        foreach (var child in node.Children())
            PrettyPrint(writer, child, indent, child == lastChild);
    }

    private static void WriteProperties(TextWriter writer, BoundNode node, bool consoleOut) {
        var firstProperty = true;

        foreach (var (Name, Value) in node.Properties()) {
            if (firstProperty)
                firstProperty = false;

            else {
                if (consoleOut)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                writer.Write(",");
            }

            writer.Write(" ");

            if (consoleOut)
                Console.ForegroundColor = ConsoleColor.Yellow;

            writer.Write(Name);

            if (consoleOut)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            
            writer.Write(": ");
            
            if (consoleOut)
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            
            writer.Write(Value);
        }
    }
}