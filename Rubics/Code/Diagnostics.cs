
using System.Collections;
using Rubics.Code.Syntax;

namespace Rubics.Code;

public readonly struct TextSpan(int start, int length) {
    public int Start { get; } = start;
    public int Length { get; } = length;
    public readonly int End => Start + Length;
}

public sealed class DiagnosticMessage(TextSpan span, string message) {
    public TextSpan Span { get; } = span;
    public string Message { get; } = message;

    public override string ToString() => Message;
}

public sealed class Diagnostics : IEnumerable<DiagnosticMessage> {
    
    public IEnumerator<DiagnosticMessage> GetEnumerator() => diagnostics.GetEnumerator();

    public void ReportInvalidNumber(TextSpan span, string literal, Type type) {
        var message = $"Number {literal} is not valid {type}";
        Report(span, message);
    }

    public void ReportBadCharacter(int position, char character) {
        var message = $"Bad character input: '{character}'";
        Report(new TextSpan(position, 1), message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind actual, SyntaxKind expected) {
        var message = $"Unexpected token <{actual}>, expected <{expected}>";
        Report(span, message);
    }

    public void ReportUndefinedUnaryOp(TextSpan span, string literal, Type type) {
        var message = $"Unary operator '{literal}' is not defined for type {type}";
        Report(span, message);
    }

    public void ReportUndefinedBinaryOp(TextSpan span, string literal, Type left, Type right) {
        var message = $"Binary operator '{literal}' is not defined for types {left} and {right}";
        Report(span, message);
    }

    private void Report(TextSpan span, string message) {
        var diagnostic = new DiagnosticMessage(span, message);
        diagnostics.Add(diagnostic);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    private readonly List<DiagnosticMessage> diagnostics = [];
}