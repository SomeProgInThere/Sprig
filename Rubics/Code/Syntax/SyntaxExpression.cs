
namespace Rubics.Code.Syntax;

public abstract class SyntaxExpression : SyntaxNode {}

internal sealed class LiteralExpression(Token literalToken, object? value)
    : SyntaxExpression {

    public LiteralExpression(Token literalToken) 
        : this(literalToken, literalToken.Value) {}

    public Token LiteralToken { get; } = literalToken;
    public object? Value { get; } = value;

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public override IEnumerable<SyntaxNode> Children() {
        yield return LiteralToken;
    }
}

internal sealed class UnaryExpression(SyntaxExpression operand, Token operatorToken)
    : SyntaxExpression {

    public SyntaxExpression Operand { get; } = operand;
    public Token OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
    public override IEnumerable<SyntaxNode> Children() {
        yield return Operand;
        yield return OperatorToken;
    }
}

internal sealed class BinaryExpression(SyntaxExpression left, SyntaxExpression right, Token operatorToken)
    : SyntaxExpression {

    public SyntaxExpression Left { get; } = left;
    public SyntaxExpression Right { get; } = right;
    public Token OperatorToken { get; } = operatorToken;

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public override IEnumerable<SyntaxNode> Children() {
        yield return Left;
        yield return Right;
        yield return OperatorToken;
    }
}

internal sealed class ParenthesizedExpression(Token open, Token closed, SyntaxExpression expression)
    : SyntaxExpression {

    public Token Open { get; } = open;
    public Token Closed { get; } = closed;
    public SyntaxExpression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
    public override IEnumerable<SyntaxNode> Children() {
        yield return Open;
        yield return Closed;
        yield return Expression;
    }
}

