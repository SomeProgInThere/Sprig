namespace Sprig.Codegen.Syntax;

public abstract class Expression : SyntaxNode {}

internal sealed class LiteralExpression(SyntaxToken literal, object? value)
    : Expression {

    public LiteralExpression(SyntaxToken literal) 
        : this(literal, literal.Value) {}

    public SyntaxToken Literal { get; } = literal;
    public object? Value { get; } = value;

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
}

public sealed class NameExpression(SyntaxToken identifier) 
    : Expression {
    
    public SyntaxToken Identifier { get; } = identifier;

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
}

public sealed class AssignmentExpression(SyntaxToken identifier, SyntaxToken equalsToken, Expression expression)
    : Expression {
    
    public SyntaxToken Identifier { get; } = identifier;
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

internal sealed class BinaryExpression(Expression left, SyntaxToken operatorToken, Expression right)
    : Expression {

    public Expression Left { get; } = left;
    public SyntaxToken OperatorToken { get; } = operatorToken;
    public Expression Right { get; } = right;

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
}

internal sealed class ParenthesizedExpression(
    SyntaxToken openParenthesisToken, Expression expression, SyntaxToken closedParenthesisToken
) : Expression {

    public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
    public Expression Expression { get; } = expression;
    public SyntaxToken ClosedParenthesisToken { get; } = closedParenthesisToken;

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

internal sealed class CallExpression(
    SyntaxToken identifier, 
    SyntaxToken openParenthesisToken,
    SeparatedSyntaxList<Expression> arguments,
    SyntaxToken closedParenthesisToken
)
    : Expression {

    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
    public SeparatedSyntaxList<Expression> Arguments { get; } = arguments;
    public SyntaxToken ClosedParenthesisToken { get; } = closedParenthesisToken;

    public override SyntaxKind Kind => SyntaxKind.CallExpression;
}