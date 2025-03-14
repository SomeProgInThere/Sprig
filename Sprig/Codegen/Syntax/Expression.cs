namespace Sprig.Codegen.Syntax;

public abstract class Expression(SyntaxTree syntaxTree) 
    : SyntaxNode(syntaxTree) {}

internal sealed class LiteralExpression(SyntaxTree syntaxTree, SyntaxToken literal, object? value)
    : Expression(syntaxTree) {

    public LiteralExpression(SyntaxTree syntaxTree, SyntaxToken literal) 
        : this(syntaxTree, literal, literal.Value) {}

    public SyntaxToken Literal { get; } = literal;
    public object? Value { get; } = value;

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
}

public sealed class NameExpression(SyntaxTree syntaxTree, SyntaxToken identifier) 
    : Expression(syntaxTree) {
    
    public SyntaxToken Identifier { get; } = identifier;

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
}

public sealed class AssignmentExpression(
    SyntaxTree syntaxTree, 
    SyntaxToken identifier, 
    SyntaxToken equalsToken, 
    Expression expression
)
    : Expression(syntaxTree) {
    
    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken EqualsToken { get; } = equalsToken;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
}

internal sealed class UnaryExpression(
    SyntaxTree syntaxTree, Expression operand, SyntaxToken operatorToken
) : Expression(syntaxTree) {

    public Expression Operand { get; } = operand;
    public SyntaxToken OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
}

internal sealed class BinaryExpression(
    SyntaxTree syntaxTree, Expression left, SyntaxToken operatorToken, Expression right
) : Expression(syntaxTree) {

    public Expression Left { get; } = left;
    public SyntaxToken OperatorToken { get; } = operatorToken;
    public Expression Right { get; } = right;

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
}

internal sealed class ParenthesizedExpression(
    SyntaxTree syntaxTree, 
    SyntaxToken openParenthesisToken, 
    Expression expression, 
    SyntaxToken closedParenthesisToken
) 
    : Expression(syntaxTree) {

    public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
    public Expression Expression { get; } = expression;
    public SyntaxToken ClosedParenthesisToken { get; } = closedParenthesisToken;

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
}

internal sealed class RangeExpression(
    SyntaxTree syntaxTree, Expression lowerBound, SyntaxToken rangeToken, Expression upperBound
) : Expression(syntaxTree)
{
    public Expression Lower { get; } = lowerBound;
    public SyntaxToken RangeToken { get; } = rangeToken;
    public Expression Upper { get; } = upperBound;

    public override SyntaxKind Kind => SyntaxKind.RangeExpression;
}

internal sealed class CallExpression(
    SyntaxTree syntaxTree, 
    SyntaxToken identifier, 
    SyntaxToken openParenthesisToken,
    SeparatedSyntaxList<Expression> arguments,
    SyntaxToken closedParenthesisToken
)
    : Expression(syntaxTree) {

    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
    public SeparatedSyntaxList<Expression> Arguments { get; } = arguments;
    public SyntaxToken ClosedParenthesisToken { get; } = closedParenthesisToken;

    public override SyntaxKind Kind => SyntaxKind.CallExpression;
}