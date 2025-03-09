
using System.CodeDom.Compiler;
using Sprig.Codegen.Syntax;

namespace Sprig.IO;

public static class TextWritterExtension {

    public static bool IsConsoleOut(this TextWriter writer) {
        if (writer == Console.Out)
            return true;

        return writer is IndentedTextWriter identedWriter 
            && identedWriter.InnerWriter.IsConsoleOut();
    }

    public static void SetForeground(this TextWriter writer, ConsoleColor color) {
        if (writer.IsConsoleOut())
            Console.ForegroundColor = color;
    }

    public static void ResetColor(this TextWriter writer) {
        if (writer.IsConsoleOut())
            Console.ResetColor();
    }

    public static void WriteSpace(this TextWriter writer) => writer.Write(" ");

    public static void WriteKeyword(this TextWriter writer, SyntaxKind token) {
        writer.SetForeground(ConsoleColor.Blue);
        writer.Write(token.Literal());
        writer.ResetColor();
    }

    public static void WriteIdentifier(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.DarkYellow);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteExtra(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.Gray);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteNumber(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.DarkCyan);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteString(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.DarkGreen);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WritePunctuation(this TextWriter writer, SyntaxKind token) {
        writer.SetForeground(ConsoleColor.Gray);
        writer.Write(token.Literal());
        writer.ResetColor();
    }
}