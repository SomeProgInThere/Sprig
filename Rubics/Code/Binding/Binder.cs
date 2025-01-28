
using Rubics.Code.Syntax;

namespace Rubics.Code.Binding;

internal sealed class Binder(Dictionary<string, object> variables) {

    public BoundExpression BindExpression(Expression syntax) {
        return syntax.Kind switch {
            SyntaxKind.LiteralExpression       => BindLiteralExpression((LiteralExpression)syntax),
            SyntaxKind.NameExpression          => BindNameExpression((NameExpression)syntax),
            SyntaxKind.AssignmentExpression    => BindAssignmentExpression((AssignmentExpression)syntax),
            SyntaxKind.UnaryExpression         => BindUnaryExpression((UnaryExpression)syntax),
            SyntaxKind.BinaryExpression        => BindBinaryExpression((BinaryExpression)syntax),
            SyntaxKind.ParenthesizedExpression => BindExpression(((ParenthesizedExpression)syntax).Expression),
            
            _ => throw new Exception($"Unexpected expression: {syntax.Kind}"),
        };
    }

    public Diagnostics Diagnostics => diagnostics;

    private static BoundExpression BindLiteralExpression(LiteralExpression syntax) {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindNameExpression(NameExpression syntax) {
        var token = syntax.IdentifierToken;
        
        if (!variables.TryGetValue(token.Literal, out var value)) {
            diagnostics.ReportUndefinedName(token.Span, token.Literal);
            return new BoundLiteralExpression(0);
        }

        var type = value?.GetType() ?? typeof(object);
        return new BoundVariableExpression(token.Literal, type);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpression syntax){
        var name = syntax.IdentifierToken.Literal;
        var expression = BindExpression(syntax.Expression);

        object? value;
        if (expression.Type == typeof(int))
            value = 0;
        else if (expression.Type == typeof(bool))
            value = false;
        else
            value = null;
        
        if (value == null) 
            throw new Exception($"Unsupported variable type: {expression.Type}");
        
        variables[name] = value;
        return new BoundAssignmentExpression(name, expression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpression syntax) {
        var operand = BindExpression(syntax.Operand);
        var token = syntax.OperatorToken;
        var op = UnaryOperator.Bind(token.Kind, operand.Type);

        if (op == null) {
            diagnostics.ReportUndefinedUnaryOp(token.Span, token.Literal, operand.Type);
            return operand;
        }

        return new BoundUnaryExpression(operand, op);
    }

    private BoundExpression BindBinaryExpression(BinaryExpression syntax) {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);
        var token = syntax.OperatorToken;
        var op = BinaryOperator.Bind(token.Kind, left.Type, right.Type);

        if (op == null) {
            diagnostics.ReportUndefinedBinaryOp(token.Span, token.Literal, left.Type, right.Type);
            return left;
        }

        return new BoundBinaryExpression(left, right, op);
    }

    private readonly Diagnostics diagnostics = [];
}