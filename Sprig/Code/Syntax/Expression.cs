namespace Sprig.Code.Syntax;

public abstract class Expression : SyntaxNode {}

internal sealed class LiteralExpression(SyntaxToken literalToken, object? value)
    : Expression {

    public LiteralExpression(SyntaxToken literalToken) 
        : this(literalToken, literalToken.Value) {}

    public SyntaxToken LiteralToken { get; } = literalToken;
    public object? Value { get; } = value;

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
}

public sealed class NameExpression(SyntaxToken identifierToken) 
    : Expression {
    
    public SyntaxToken IdentifierToken { get; } = identifierToken;

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
}

public sealed class AssignmentExpression(SyntaxToken identifierToken, SyntaxToken equalsToken, Expression expression)
    : Expression {
    
    public SyntaxToken IdentifierToken { get; } = identifierToken;
    public SyntaxToken EqualsToken { get; } = equalsToken;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
}

internal sealed class UnaryExpression(Expression operand, SyntaxToken operatorToken)
    : Expression {

    public Expression Operand { get; } = operand;
    public SyntaxToken OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
}

internal sealed class BinaryExpression(Expression left, Expression right, SyntaxToken operatorToken)
    : Expression {

    public Expression Left { get; } = left;
    public Expression Right { get; } = right;
    public SyntaxToken OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
}

internal sealed class ParenthesizedExpression(SyntaxToken open, SyntaxToken closed, Expression expression)
    : Expression {

    public SyntaxToken Open { get; } = open;
    public SyntaxToken Closed { get; } = closed;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
}

internal sealed class RangeExpression(Expression lowerBound, SyntaxToken rangeToken, Expression upperBound)
    : Expression
{
    public Expression LowerBound { get; } = lowerBound;
    public SyntaxToken RangeToken { get; } = rangeToken;
    public Expression UpperBound { get; } = upperBound;

    public override SyntaxKind Kind => SyntaxKind.RangeExpression;
}