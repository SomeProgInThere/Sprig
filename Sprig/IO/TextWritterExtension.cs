
using System.CodeDom.Compiler;
using Sprig.Codegen;
using Sprig.Codegen.Text;
using Sprig.Codegen.Syntax;

namespace Sprig.IO;

public static class TextWritterExtension {

    public static bool IsColorAvailable(this TextWriter writer) {
        if (writer == Console.Out || writer == Console.Error)
            return true;

        return writer is IndentedTextWriter identedWriter 
            && identedWriter.InnerWriter.IsColorAvailable();
    }

    public static void SetForeground(this TextWriter writer, ConsoleColor color) {
        if (writer.IsColorAvailable())
            Console.ForegroundColor = color;
    }

    public static void ResetColor(this TextWriter writer) {
        if (writer.IsColorAvailable())
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

    public static void WriteInfo(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.Gray);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteError(this TextWriter writer, string text) {
        writer.SetForeground(ConsoleColor.DarkRed);
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

    public static void WriteDiagnostics(this TextWriter writer, IEnumerable<DiagnosticMessage> diagnostics) {
        
        foreach (var diagnostic in diagnostics.Where(diag => diag.Location.Source is null)) {
            writer.WriteError(diagnostic.Message);
            writer.WriteLine();
        }

        var orderedDiagnostics = diagnostics
            .Where(diag => diag.Location.Source != null)
            .OrderBy(diag => diag.Location.FileName)
            .OrderBy(diag => diag.Location.Span.Start)
            .ThenBy(diag => diag.Location.Span.End);

        foreach (var diagnostic in orderedDiagnostics) {

            var fileName = diagnostic.Location.FileName;
            var source = diagnostic.Location.Source;
            var span = diagnostic.Location.Span;

            var lineIndex = source.GetLineIndex(span.Start);
            var line = source.Lines[lineIndex];
            var column = span.Start - line.Start + 1;
            var lineNumber = lineIndex + 1;

            writer.WriteLine();
            writer.WriteError($"{fileName} ({lineNumber}, {column}) Error: ");
            writer.WriteInfo($"{diagnostic}\n");

            var prefixSpan = TextSpan.CreateFromBounds(line.Start, span.Start);
            var suffixSpan = TextSpan.CreateFromBounds(span.End, line.End);

            var prefix = source.ToString(prefixSpan).TrimStart();
            var error = source.ToString(span);
            var suffix = source.ToString(suffixSpan);

            writer.WriteInfo($"\t--> {lineNumber, 1} | {prefix}");
            writer.WriteError(error);
            writer.WriteInfo($"{suffix}\n");
        }

        writer.WriteLine();
    }
}