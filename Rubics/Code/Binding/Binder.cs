
using System.Collections.Immutable;
using Rubics.Code.Syntax;

namespace Rubics.Code.Binding;

internal sealed class Binder(BoundScope? parent) {

    private BoundStatement BindStatement(Statement syntax) {
        return syntax.Kind switch {
            SyntaxKind.BlockStatment        => BindBlockStatement((BlockStatement)syntax),
            SyntaxKind.VariableDeclaration  => BindVariableDeclaration((VariableDeclarationStatement)syntax),
            SyntaxKind.ExpressionStatement  => BindExpressionStatement((ExpressionStatement)syntax),

            _ => throw new Exception($"Unexpected statement: {syntax.Kind}"),
        };
    }

    private BoundStatement BindBlockStatement(BlockStatement syntax) {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        Scope = new BoundScope(Scope);

        foreach (var statement in syntax.Statements) {
            var boundStatement = BindStatement(statement);
            statements.Add(boundStatement);
        }

        Scope = Scope.Parent ?? new BoundScope(parent);
        return new BoundBlockStatement(statements.ToImmutable());
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationStatement syntax) {
        var name = syntax.Identifier.Literal;
        var mutable = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var initializer = BindExpression(syntax.Initializer);

        var variable = new VariableSymbol(name, mutable, initializer.Type);

        if (!Scope.TryDeclare(variable))
            diagnostics.ReportVariableRedeclaration(syntax.Identifier.Span, name);

        return new BoundVariableDeclarationStatement(variable, initializer);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatement syntax) {
        var expression = BindExpression(syntax.Expression);
        return new BoundExpressionStatement(expression);
    }

    private BoundExpression BindExpression(Expression syntax) {
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
        
        var statement = binder.BindStatement(compilation.Statement);
        var variables = binder.Scope.Variables;
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        return new BoundGlobalScope(previous, diagnostics, variables, statement);
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
    public BoundScope Scope = new(parent);

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

    private BoundExpression BindAssignmentExpression(AssignmentExpression syntax) {
        var name = syntax.IdentifierToken.Literal;
        var expression = BindExpression(syntax.Expression);

        if (!Scope.TryLookup(name, out var variable)) {
            diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return expression;
        }

        if (variable?.Mutable ?? false)
            diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

        if (expression.Type != variable?.Type) {
            diagnostics.ReportCannotConvert(syntax.Expression.Span, expression.Type, variable?.Type);
            return expression;
        }

        return new BoundAssignmentExpression(variable, expression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpression syntax) {
        var operand = BindExpression(syntax.Operand);
        var token = syntax.OperatorToken;
        var op = UnaryOperator.Bind(token.Kind, operand.Type);

        if (op == null) {
            diagnostics.ReportUndefinedUnaryOperator(token.Span, token.Literal, operand.Type);
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
            diagnostics.ReportUndefinedBinaryOperator(token.Span, token.Literal, left.Type, right.Type);
            return left;
        }

        return new BoundBinaryExpression(left, right, op);
    }

    private readonly Diagnostics diagnostics = [];
}