
namespace Sprig.Code.Syntax;

public abstract class Expression : SyntaxNode {}

internal sealed class LiteralExpression(Token literalToken, object? value)
    : Expression {

    public LiteralExpression(Token literalToken) 
        : this(literalToken, literalToken.Value) {}

    public Token LiteralToken { get; } = literalToken;
    public object? Value { get; } = value;

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
}

public sealed class NameExpression(Token identifierToken) 
    : Expression {
    
    public Token IdentifierToken { get; } = identifierToken;

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
}

public sealed class AssignmentExpression(Token identifierToken, Token equalsToken, Expression expression)
    : Expression {
    
    public Token IdentifierToken { get; } = identifierToken;
    public Token EqualsToken { get; } = equalsToken;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
}

internal sealed class UnaryExpression(Expression operand, Token operatorToken)
    : Expression {

    public Expression Operand { get; } = operand;
    public Token OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
}

internal sealed class BinaryExpression(Expression left, Expression right, Token operatorToken)
    : Expression {

    public Expression Left { get; } = left;
    public Expression Right { get; } = right;
    public Token OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
}

internal sealed class ParenthesizedExpression(Token open, Token closed, Expression expression)
    : Expression {

    public Token Open { get; } = open;
    public Token Closed { get; } = closed;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
}

internal sealed class RangeExpression(Expression lowerBound, Token rangeToken, Expression upperBound)
    : Expression
{
    public Expression LowerBound { get; } = lowerBound;
    public Token RangeToken { get; } = rangeToken;
    public Expression UpperBound { get; } = upperBound;

    public override SyntaxKind Kind => SyntaxKind.RangeExpression;
}