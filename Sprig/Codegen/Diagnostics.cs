using System.Collections;

using Sprig.Codegen.Text;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen;

public sealed class Diagnostics : IEnumerable<DiagnosticMessage> {
    
    public IEnumerator<DiagnosticMessage> GetEnumerator() => diagnostics.GetEnumerator();

    public void ReportInvalidNumber(TextLocation location, string literal, TypeSymbol type) {
        var message = $"Number '{literal}' is not valid '{type}'";
        Report(location, message);
    }

    public void ReportBadCharacter(TextLocation location, char character) {
        var message = $"Bad character input: '{character}'";
        Report(location, message);
    }

    public void ReportUnterminatedString(TextLocation location) {
        var message = "Unterminated string literal";
        Report(location, message);
    }

    public void ReportUnexpectedToken(TextLocation location, SyntaxKind actual, SyntaxKind expected) {
        var actualString = SyntaxKindExtension.Literal(actual) is null 
            ? $"<{actual}>" 
            : $"'{SyntaxKindExtension.Literal(actual)}'";
        
        var expectedString = SyntaxKindExtension.Literal(expected) is null 
            ? $"<{expected}>" 
            : $"'{SyntaxKindExtension.Literal(expected)}'";
        
        var message = $"Unexpected token {actualString}, expected {expectedString}";
        Report(location, message);
    }

    public void ReportUndefinedUnaryOperator(TextLocation location, string literal, TypeSymbol type) {
        var message = $"Unary operator '{literal}' is not defined for type '{type}'";
        Report(location, message);
    }

    public void ReportUndefinedBinaryOperator(TextLocation location, string literal, TypeSymbol left, TypeSymbol right) {
        var message = $"Binary operator '{literal}' is not defined for types '{left}' and '{right}'";
        Report(location, message);
    }

    public void ReportUndefinedIdentifier(TextLocation location, string literal) {
        var message = $"Identifier '{literal}' is not defined in scope";
        Report(location, message);
    }

    public void ReportUndefinedType(TextLocation location, string literal) {
        var message = $"Type '{literal}' is not defined in scope";
        Report(location, message);
    }

    public void ReportVariableRedeclaration(TextLocation location, string name) {
        var message = $"Variable '{name}' is already declared in scope";
        Report(location, message);
    }

    public void ReportCannotConvert(TextLocation location, TypeSymbol actual, TypeSymbol? expected, bool reportCastExisits = false) {
        var message = $"Cannot convert type '{actual}' to '{expected}'";
        if (reportCastExisits)
            message += ". An explicit cast exists, are you missing a cast?";

        Report(location, message);
    }

    public void ReportCannotAssign(TextLocation location, string name) {
        var message = $"Cannot assign value to immutabe variable '{name}'";
        Report(location, message);
    }

    public void ReportNonIntegerRange(TextLocation location) {
        var message = $"Range expression initialized with non-integer type";
        Report(location, message);
    }

    public void ReportUndefinedFunction(TextLocation location, string name) {
        var message = $"Function '{name}' does not exist";
        Report(location, message);
    }

    public void ReportIncorrectArgumentCount(TextLocation location, string name, int expected, int actual) {
        var message = $"Function '{name}' needs '{expected}' argument(s), but '{actual}' were provided";
        Report(location, message);
    }

    public void ReportVoidExpression(TextLocation location) {
        var message = $"Expression must have a return value";
        Report(location, message);
    }

    public void ReportSymbolAlreadyExists(TextLocation location, string name) {
        var message = $"Symbol '{name}' already exists";
        Report(location, message);
    }

    public void ReportParameterAlreadyExists(TextLocation location, string name) {
        var message = $"Parameter '{name}' already exists";
        Report(location, message);
    }
    
    public void ReportInvalidJump(TextLocation location, string literal) {
        var message = $"No enclosing loop to {literal} out of";
        Report(location, message);
    }

    public void ReportInvalidReturnExpression(TextLocation location, string name) {
        var message = $"Keyword 'return' cannot be used for void function '{name}'";
        Report(location, message);
    }

    public void ReportInvalidReturnGlobalStatement(TextLocation location) {
        var message = $"Keyword 'return' cannot be used in global statements";
        Report(location, message);
    }
    
    public void ReportMissingReturnExpression(TextLocation location, string name, TypeSymbol type) {
        var message = $"A return expression of type '{type}' expected for function '{name}'";
        Report(location, message);
    }

    public void ReportNotAllPathsReturn(TextLocation location) {
        var message = $"Not all code paths return a value";
        Report(location, message);
    }

    public void ReportUndefinedVariable(TextLocation location, string name) {
        var message = $"Variable '{name}' does not exist";
        Report(location, message);
    }

    public void ReportNotAVariable(TextLocation location, string name) {
        var message = $"Symbol '{name}' is not a variable";
        Report(location, message);
    }
    
    public void ReportNotAFunction(TextLocation location, string name) {
        var message = $"Symbol '{name}' is not a function";
        Report(location, message);
    }

    public void ReportInvalidExpressionStatement(TextLocation location) {
        var message = $"Only assignment and call expressions can be used as a statement";
        Report(location, message);
    }
    
    public void ReportMainAlreadyExists(TextLocation location) {
        var message = $"A 'main' function already exists. Global statements cannot be used";
        Report(location, message);
    }
    
    public void ReportIncorrectMainDefinition(TextLocation location) {
        var message = $"Incorrect 'main' function definition. Should not conatin any parameters or return statement";
        Report(location, message);
    }

    public void ReportMultipleGlobalStatements(TextLocation location) {
        var message = $"Only one file can contain global statements";
        Report(location, message);
    }

    private void Report(TextLocation location, string message) {
        var diagnostic = new DiagnosticMessage(location, message);
        diagnostics.Add(diagnostic);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    private readonly List<DiagnosticMessage> diagnostics = [];
}

public sealed class DiagnosticMessage(TextLocation location, string message) {
    public TextLocation Location { get; } = location;
    public string Message { get; } = message;

    public override string ToString() => Message;
}