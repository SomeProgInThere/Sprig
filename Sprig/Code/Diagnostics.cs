using System.Collections;

using Sprig.Code.Source;
using Sprig.Code.Syntax;

namespace Sprig.Code;

public sealed class Diagnostics : IEnumerable<DiagnosticMessage> {
    
    public IEnumerator<DiagnosticMessage> GetEnumerator() => diagnostics.GetEnumerator();

    public void ReportInvalidNumber(TextSpan span, string literal, TypeSymbol type) {
        var message = $"Number '{literal}' is not valid '{type}'";
        Report(span, message);
    }

    public void ReportBadCharacter(TextSpan span, char character) {
        var message = $"Bad character input: '{character}'";
        Report(span, message);
    }

    public void ReportUnterminatedString(TextSpan span) {
        var message = "Unterminated string literal";
        Report(span, message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind actual, SyntaxKind expected) {
        var actualString = actual.Literal() is null ? $"<{actual}>" : $"'{actual.Literal()}'";
        var expectedString = expected.Literal() is null ? $"<{expected}>" : $"'{expected.Literal()}'";
        var message = $"Unexpected token {actualString}, expected {expectedString}";
        
        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan span, string literal, TypeSymbol type) {
        var message = $"Unary operator '{literal}' is not defined for type '{type}'";
        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string literal, TypeSymbol left, TypeSymbol right) {
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

    public void ReportCannotConvert(TextSpan span, TypeSymbol actual, TypeSymbol? expected) {
        var message = $"Cannot convert type '{actual}' to '{expected}'";
        Report(span, message);
    }

    public void ReportCannotAssign(TextSpan span, string name) {
        var message = $"Cannot assign value to immutabe variable '{name}'";
        Report(span, message);
    }

    public void ReportNonIntegerRange(TextSpan span) {
        var message = $"Range expression initialized with non-integer type";
        Report(span, message);
    }

    public void ReportUndefinedFunctionCall(TextSpan span, string name) {
        var message = $"Function '{name}' is not defined";
        Report(span, message);
    }

    public void ReportIncorrectArgumentCount(TextSpan span, string name, int expected, int actual) {
        var message = $"Function '{name}' needs '{expected}' argument(s), but '{actual}' were provided";
        Report(span, message);
    }

    public void ReportIncorrectArgumentType(TextSpan span, string name, TypeSymbol expected, TypeSymbol actual) {
        var message = $"Parameter '{name}' cannot be converted from '{actual}' to '{expected}'";
        Report(span, message);
    }

    public void ReportVoidExpression(TextSpan span) {
        var message = $"Expression must have a return value";
        Report(span, message);
    }

    private void Report(TextSpan span, string message) {
        var diagnostic = new DiagnosticMessage(span, message);
        diagnostics.Add(diagnostic);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    private readonly List<DiagnosticMessage> diagnostics = [];
}

public sealed class DiagnosticMessage(TextSpan span, string message) {
    public TextSpan Span { get; } = span;
    public string Message { get; } = message;

    public override string ToString() => Message;
}