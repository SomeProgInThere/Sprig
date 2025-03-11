using System.Collections.Immutable;

namespace Sprig.Codegen.Syntax;

public class CompilationUnit(ImmutableArray<Member> members, SyntaxToken endOfFileToken) 
    : SyntaxNode {

    public ImmutableArray<Member> Members { get; } = members;
    public SyntaxToken EndOfFileToken { get; } = endOfFileToken;

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}

public abstract class Member : SyntaxNode {}

public sealed class GlobalStatment(Statement statement)
    : Member {
    
    public Statement Statement { get; } = statement;

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
}

public sealed class FunctionParameter(SyntaxToken identifier, TypeClause type) 
    : SyntaxNode {

    public SyntaxToken Identifier { get; } = identifier;
    public TypeClause Type { get; } = type;

    public override SyntaxKind Kind => SyntaxKind.FunctionParameter;
}

public sealed class FunctionHeader(
    SyntaxToken funcKeyword, 
    SyntaxToken identifier, 
    SyntaxToken openParenthesisToken, 
    SeparatedSyntaxList<FunctionParameter> parameters,
    SyntaxToken closedParenthesisToken,
    TypeClause returnType,
    BlockStatement body
) : Member {

    public SyntaxToken FuncKeyword { get; } = funcKeyword;
    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
    public SeparatedSyntaxList<FunctionParameter> Parameters { get; } = parameters;
    public SyntaxToken ClosedParenthesisToken { get; } = closedParenthesisToken;
    public TypeClause ReturnType { get; } = returnType;
    public BlockStatement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.FunctionHeader;
}