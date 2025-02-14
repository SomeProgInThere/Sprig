
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

public sealed class VariableDeclarationStatement(Token keyword, Token identifier, Token equalsToken, Expression initializer)
    : Statement {

    public Token Keyword { get; } = keyword;
    public Token Identifier { get; } = identifier;
    public Token EqualsToken { get; } = equalsToken;
    public Expression Initializer { get; } = initializer;

    public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
}

public sealed class IfStatement(Token ifToken, Expression condition, Statement body, ElseClause? elseClause) 
    : Statement {

    public Token IfToken { get; } = ifToken;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
    public ElseClause? ElseClause { get; } = elseClause;

    public override SyntaxKind Kind => SyntaxKind.IfStatement;
}

public sealed class WhileStatement(Token whileToken, Expression condition, Statement body) 
    : Statement {

    public Token WhileToken { get; } = whileToken;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
}