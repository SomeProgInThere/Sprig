using System.Collections;

using Sprig.Codegen.Text;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;
using Mono.Cecil;

namespace Sprig.Codegen;

public sealed class Diagnostics : IEnumerable<DiagnosticMessage> {
    
    // Language Errors

    public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type) {
        var message = $"Number '{text}' is not valid '{type}'";
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
        var actualString = SyntaxKindExtension.Text(actual) is null 
            ? $"<{actual}>" 
            : $"'{SyntaxKindExtension.Text(actual)}'";
        
        var expectedString = SyntaxKindExtension.Text(expected) is null 
            ? $"<{expected}>" 
            : $"'{SyntaxKindExtension.Text(expected)}'";
        
        var message = $"Unexpected token {actualString}, expected {expectedString}";
        Report(location, message);
    }

    public void ReportUndefinedUnaryOperator(TextLocation location, string text, TypeSymbol type) {
        var message = $"Unary operator '{text}' is not defined for type '{type}'";
        Report(location, message);
    }

    public void ReportUndefinedBinaryOperator(TextLocation location, string text, TypeSymbol left, TypeSymbol right) {
        var message = $"Binary operator '{text}' is not defined for types '{left}' and '{right}'";
        Report(location, message);
    }

    public void ReportUndefinedIdentifier(TextLocation location, string text) {
        var message = $"Identifier '{text}' is not defined in scope";
        Report(location, message);
    }

    public void ReportUndefinedType(TextLocation location, string text) {
        var message = $"Type '{text}' is not defined in scope";
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
    
    public void ReportInvalidJump(TextLocation location, string text) {
        var message = $"No enclosing loop to {text} out of";
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

    // .NET Assembly Linkage Errors

    public void ReportInvalidReferenceLinking(string path) {
        var message = $"Cannot link invalid .NET assembly: '{path}'";
        Report(default, message);
    }

    public void ReportRequiredTypeNotFound(string? typeName, string metadataName) {
        var message = typeName is null 
            ? $"Required type '{metadataName}' could not be resolved from references"
            : $"Required type '{typeName}' ('{metadataName}') could not be resolved from references";

        Report(default, message);
    }

    public void ReportRequiredTypeAmbiguous(string? typeName, string metadataName, TypeDefinition[] foundTypes) {
        var assemblyName = foundTypes.Select(t => t.Module.Assembly.Name.Name);
        var assemblyNameList = string.Join("', '", assemblyName);

        var message = typeName is null 
            ? $"Required type '{metadataName}' was found in multiple references: {assemblyNameList}"
            : $"Required type '{typeName}' ('{metadataName}') was found in multiple references: {assemblyNameList}";

        Report(default, message);
    }
    
    public void ReportRequiredMethodNotFound(string typeName, string methodName, string[] parameterTypeNames) {
        var parameterTypeNameList = string.Join(", ", parameterTypeNames);
        var message = $"Required method '{typeName}.{methodName}({parameterTypeNameList})' could not be resolved from references";
        Report(default, message);
    }

    private void Report(TextLocation location, string message) {
        var diagnostic = new DiagnosticMessage(location, message);
        diagnostics.Add(diagnostic);
    }

    public IEnumerator<DiagnosticMessage> GetEnumerator() => diagnostics.GetEnumerator();

    private readonly List<DiagnosticMessage> diagnostics = [];
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed class DiagnosticMessage(TextLocation location, string message) {
    public TextLocation Location { get; } = location;
    public string Message { get; } = message;

    public override string ToString() => Message;
}