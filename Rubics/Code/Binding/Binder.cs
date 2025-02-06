
using System.Collections.Immutable;
using Rubics.Code.Syntax;

namespace Rubics.Code.Binding;

internal sealed class Binder(BoundScope? parent) {

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

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnit compilation) {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope);
        
        var expression = binder.BindExpression(compilation.Expression);
        var variables = binder.Scope.Variables;
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        return new BoundGlobalScope(previous, diagnostics, variables, expression);
    }

    private static BoundScope? CreateParentScope(BoundGlobalScope? previous) {
        
        var stack = new Stack<BoundGlobalScope>();
        while (previous != null) {
            stack.Push(previous);
            previous = previous.Previous;
        }

        BoundScope? parent = null;
        while (stack.Count > 0) {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            
            foreach (var variable in previous.Variables)
                scope.TryDeclare(variable);

            parent = scope;
        }

        return parent;
    }

    public Diagnostics Diagnostics => diagnostics;
    public BoundScope Scope { get; } = new BoundScope(parent);

    private static BoundExpression BindLiteralExpression(LiteralExpression syntax) {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindNameExpression(NameExpression syntax) {
        var token = syntax.IdentifierToken;

        if (!Scope.TryLookup(token.Literal, out var variable)) {
            diagnostics.ReportUndefinedName(token.Span, token.Literal);
            return new BoundLiteralExpression(0);
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpression syntax){
        var name = syntax.IdentifierToken.Literal;
        var expression = BindExpression(syntax.Expression);
        var variable = new VariableSymbol(name, expression.Type);

        if (!Scope.TryDeclare(variable))
            diagnostics.ReportVariableAlreadyDeclared(syntax.IdentifierToken.Span, name);

        return new BoundAssignmentExpression(variable, expression);
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