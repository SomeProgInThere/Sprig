
using System.Collections.Immutable;

namespace Rubics.Code.Syntax;

public abstract class Expression : SyntaxNode {}

internal sealed class LiteralExpression(Token literalToken, object? value)
    : Expression {

    public LiteralExpression(Token literalToken) 
        : this(literalToken, literalToken.Value) {}

    public Token LiteralToken { get; } = literalToken;
    public object? Value { get; } = value;

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public override ImmutableArray<SyntaxNode> Children() {
        return [LiteralToken];
    }
}

public sealed class NameExpression(Token identifierToken) 
    : Expression {
    
    public Token IdentifierToken { get; } = identifierToken;

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public override ImmutableArray<SyntaxNode> Children() {
        return [IdentifierToken];
    }
}

public sealed class AssignmentExpression(Token identifierToken, Token equalsToken, Expression expression)
    : Expression {
    
    public Token IdentifierToken { get; } = identifierToken;
    public Token EqualsToken { get; } = equalsToken;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public override ImmutableArray<SyntaxNode> Children() {
        return [IdentifierToken, EqualsToken, Expression];
    }
}

internal sealed class UnaryExpression(Expression operand, Token operatorToken)
    : Expression {

    public Expression Operand { get; } = operand;
    public Token OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
    public override ImmutableArray<SyntaxNode> Children() {
        return [Operand, OperatorToken];
    }
}

internal sealed class BinaryExpression(Expression left, Expression right, Token operatorToken)
    : Expression {

    public Expression Left { get; } = left;
    public Expression Right { get; } = right;
    public Token OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public override ImmutableArray<SyntaxNode> Children() {
        return [Left, Right, OperatorToken];
    }
}

internal sealed class ParenthesizedExpression(Token open, Token closed, Expression expression)
    : Expression {

    public Token Open { get; } = open;
    public Token Closed { get; } = closed;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    public override ImmutableArray<SyntaxNode> Children() {
        return [Open, Closed, Expression];
    }
}