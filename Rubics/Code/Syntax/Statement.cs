
using System.Collections.Immutable;

namespace Rubics.Code.Syntax;

public abstract class Statement : SyntaxNode {}

public sealed class BlockStatement(Token openBraceToken, ImmutableArray<Statement> statements, Token closedBraceToken)
    : Statement {

    public Token OpenBraceToken { get; } = openBraceToken;
    public ImmutableArray<Statement> Statements { get; } = statements;
    public Token ClosedBraceToken { get; } = closedBraceToken;

    public override SyntaxKind Kind => SyntaxKind.BlockStatment;
}

public sealed class ExpressionStatement(Expression expression)
    : Statement {
        
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
}