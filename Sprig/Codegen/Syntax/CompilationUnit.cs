using System.Collections.Immutable;

namespace Sprig.Codegen.Syntax;

public class CompilationUnit(SyntaxTree syntaxTree, ImmutableArray<Member> members, SyntaxToken endOfFileToken) 
    : SyntaxNode(syntaxTree) {

    public ImmutableArray<Member> Members { get; } = members;
    public SyntaxToken EndOfFileToken { get; } = endOfFileToken;

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}

public abstract class Member(SyntaxTree syntaxTree) 
    : SyntaxNode(syntaxTree) {}

public sealed class GlobalStatment(SyntaxTree syntaxTree, Statement statement)
    : Member(syntaxTree) {
    
    public Statement Statement { get; } = statement;

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
}

public sealed class FunctionParameter(SyntaxTree syntaxTree, SyntaxToken identifier, TypeClause type) 
    : SyntaxNode(syntaxTree) {

    public SyntaxToken Identifier { get; } = identifier;
    public TypeClause Type { get; } = type;

    public override SyntaxKind Kind => SyntaxKind.FunctionParameter;
}

public sealed class FunctionHeader(
    SyntaxTree syntaxTree,
    SyntaxToken funcKeyword, 
    SyntaxToken identifier, 
    SyntaxToken openParenthesisToken, 
    SeparatedSyntaxList<FunctionParameter> parameters,
    SyntaxToken closedParenthesisToken,
    TypeClause returnType,
    BlockStatement body
) 
    : Member(syntaxTree) {

    public SyntaxToken FuncKeyword { get; } = funcKeyword;
    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
    public SeparatedSyntaxList<FunctionParameter> Parameters { get; } = parameters;
    public SyntaxToken ClosedParenthesisToken { get; } = closedParenthesisToken;
    public TypeClause ReturnType { get; } = returnType;
    public BlockStatement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.FunctionHeader;
}