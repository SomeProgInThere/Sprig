
using Rubics.Code.Syntax;

namespace Rubics.Code.Binding;

internal sealed class Binder {

    public BoundExpression BindExpression(SyntaxExpression syntax) {
        return syntax.Kind switch {
            SyntaxKind.LiteralExpression       => BindLiteralExpression((LiteralExpression)syntax),
            SyntaxKind.UnaryExpression         => BindUnaryExpression((UnaryExpression)syntax),
            SyntaxKind.BinaryExpression        => BindBinaryExpression((BinaryExpression)syntax),
            SyntaxKind.ParenthesizedExpression => BindExpression((ParenthesizedExpression)syntax),
            _ => throw new Exception($"Unexpected expression: {syntax.Kind}"),
        };
    }
    
    public IEnumerable<string> Diagnostics => diagnostics;

    private static BoundExpression BindLiteralExpression(LiteralExpression syntax) {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindUnaryExpression(UnaryExpression syntax) {
        var operand = BindExpression(syntax.Operand);
        var op = UnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);

        if (op == null) {
            diagnostics.Add($"Unary operator '{syntax.OperatorToken.Literal}' is not defined for type '{operand.Type}'");
            return operand;
        }

        return new BoundUnaryExpression(operand, op);
    }

    private BoundExpression BindBinaryExpression(BinaryExpression syntax) {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);
        var op = BinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
        
        if (op == null) {
            diagnostics.Add($"Binary operator '{syntax.OperatorToken.Literal}' is not defined for type '{left.Type}' and '{right.Type}'");
            return left;
        }

        return new BoundBinaryExpression(left, right, op);
    }

    private UnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operatorType) {
        if (operatorType == typeof(int)) {
            switch (kind) {
                case SyntaxKind.PlusToken:  return UnaryOperatorKind.Identity;
                case SyntaxKind.MinusToken: return UnaryOperatorKind.Negetion;
            }
        }

        if (operatorType == typeof(bool)) {
            switch (kind) {
                case SyntaxKind.BangToken: return UnaryOperatorKind.LogicalNot;
            }
        }

        return null;
    }

    private static BinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType) {
        if (leftType == typeof(int) && rightType == typeof(int)) {
            switch (kind) {
                case SyntaxKind.PlusToken:    return BinaryOperatorKind.Add;
                case SyntaxKind.MinusToken:   return BinaryOperatorKind.Substact;
                case SyntaxKind.StarToken:    return BinaryOperatorKind.Multiply;
                case SyntaxKind.SlashToken:   return BinaryOperatorKind.Divide;
                case SyntaxKind.PercentToken: return BinaryOperatorKind.Modulus;
            }
        }

        if (leftType == typeof(bool) && rightType == typeof(bool)) {
            switch (kind) {
                case SyntaxKind.DoubleAmpersandToken: return BinaryOperatorKind.LogicalAnd;
                case SyntaxKind.DoublePipeToken:      return BinaryOperatorKind.LogicalOr;
            }
        }

        return null;
    }

    private readonly List<string> diagnostics = [];
}