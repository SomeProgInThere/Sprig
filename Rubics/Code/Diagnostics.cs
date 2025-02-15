
using System.Collections;
using Rubics.Code.Source;
using Rubics.Code.Syntax;

namespace Rubics.Code;

public sealed class DiagnosticMessage(TextSpan span, string message) {
    public TextSpan Span { get; } = span;
    public string Message { get; } = message;

    public override string ToString() => Message;
}

public sealed class Diagnostics : IEnumerable<DiagnosticMessage> {
    
    public IEnumerator<DiagnosticMessage> GetEnumerator() => diagnostics.GetEnumerator();

    public void ReportInvalidNumber(TextSpan span, string literal, Type type) {
        var message = $"Number '{literal}' is not valid '{type}'";
        Report(span, message);
    }

    public void ReportBadCharacter(int position, char character) {
        var message = $"Bad character input: '{character}'";
        Report(new TextSpan(position, 1), message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind actual, SyntaxKind expected) {
        var message = $"Unexpected token '{actual.GetLiteral()}', expected '{expected.GetLiteral()}'";
        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan span, string literal, Type type) {
        var message = $"Unary operator '{literal}' is not defined for type '{type}'";
        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string literal, Type left, Type right) {
        var message = $"Binary operator '{literal}' is not defined for types '{left}' and '{right}'";
        Report(span, message);
    }

    public void ReportUndefinedName(TextSpan span, string literal) {
        var message = $"Symbol '{literal}' is not defined";
        Report(span, message);
    }

    public void ReportVariableRedeclaration(TextSpan span, string name) {
        var message = $"Variable '{name}' is already declared";
        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan span, Type actual, Type? expected) {
        var message = $"Cannot convert type '{actual}' to '{expected}'";
        Report(span, message);
    }

    public void ReportCannotAssign(TextSpan span, string name) {
        var message = $"Cannot assign value to immutabe variable '{name}'";
        Report(span, message);
    }

    private void Report(TextSpan span, string message) {
        var diagnostic = new DiagnosticMessage(span, message);
        diagnostics.Add(diagnostic);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private readonly List<DiagnosticMessage> diagnostics = [];
}