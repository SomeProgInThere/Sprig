using System.Collections;

using Sprig.Codegen.Source;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen;

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
        var actualString = SyntaxKindExtension.Literal(actual) is null ? $"<{actual}>" : $"'{SyntaxKindExtension.Literal(actual)}'";
        var expectedString = SyntaxKindExtension.Literal(expected) is null ? $"<{expected}>" : $"'{SyntaxKindExtension.Literal(expected)}'";
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

    public void ReportUndefinedIdentifier(TextSpan span, string literal) {
        var message = $"Identifier '{literal}' is not defined in scope";
        Report(span, message);
    }

    public void ReportUndefinedType(TextSpan span, string literal) {
        var message = $"Type '{literal}' is not defined in scope";
        Report(span, message);
    }

    public void ReportVariableRedeclaration(TextSpan span, string name) {
        var message = $"Variable '{name}' is already declared in scope";
        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan span, TypeSymbol actual, TypeSymbol? expected, bool reportCastExisits = false) {
        var message = $"Cannot convert type '{actual}' to '{expected}'";
        if (reportCastExisits)
            message += ". An explicit cast exists (are you missing a cast?)";

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

    public void ReportUndefinedFunction(TextSpan span, string name) {
        var message = $"Function '{name}' does not exist";
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

    public void ReportSymbolAlreadyExists(TextSpan span, string name) {
        var message = $"Symbol '{name}' already exists";
        Report(span, message);
    }

    public void ReportParameterAlreadyExists(TextSpan span, string name) {
        var message = $"Parameter '{name}' already exists";
        Report(span, message);
    }
    
    public void ReportInvalidJump(TextSpan span, string literal) {
        var message = $"No enclosing loop to {literal} out of";
        Report(span, message);
    }

    public void ReportInvalidReturn(TextSpan span) {
        var message = $"Keyword 'return' is not present inside of a function";
        Report(span, message);
    }

    public void ReportInvalidReturnExpression(TextSpan span, string name) {
        var message = $"Keyword 'return' cannot be used for a non-return function"; 
        Report(span, message);
    }
    
    public void ReportMissingReturnExpression(TextSpan span, string name, TypeSymbol type) {
        var message = $"A expression of type '{type}' expected for function '{name}'";
        Report(span, message);
    }

    public void ReportNotAllPathsReturn(TextSpan span) {
        var message = $"Not all code paths return a value";
        Report(span, message);
    }

    public void ReportUndefinedVariable(TextSpan span, string name) {
        var message = $"Variable '{name}' does not exist";
        Report(span, message);
    }

    public void ReportNotAVariable(TextSpan span, string name) {
        var message = $"Symbol '{name}' is not a variable";
        Report(span, message);
    }
    
    public void ReportNotAFunction(TextSpan span, string name) {
        var message = $"Symbol '{name}' is not a function";
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