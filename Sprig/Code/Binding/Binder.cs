using System.Collections.Immutable;

using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal sealed class Binder(BoundScope? parent) {

    private BoundStatement BindStatement(Statement syntax) {
        return syntax.Kind switch {
            SyntaxKind.BlockStatment            => BindBlockStatement((BlockStatement)syntax),
            SyntaxKind.VariableDeclaration      => BindVariableDeclaration((VariableDeclarationStatement)syntax),
            SyntaxKind.AssignOperationStatement => BindAssignOperationStatement((AssignOperationStatement)syntax),
            SyntaxKind.ExpressionStatement      => BindExpressionStatement((ExpressionStatement)syntax),
            SyntaxKind.IfStatement              => BindIfStatement((IfStatement)syntax),
            SyntaxKind.WhileStatement           => BindWhileStatement((WhileStatement)syntax),
            SyntaxKind.ForStatement             => BindForStatement((ForStatement)syntax),

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
        var mutable = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var initializer = BindExpression(syntax.Initializer);
        var variable = BindVariable(syntax.Identifier, mutable, initializer.Type);

        return new BoundVariableDeclarationStatement(variable, initializer);
    }

    private BoundStatement BindAssignOperationStatement(AssignOperationStatement syntax) {
        var name = syntax.Identifier.LiteralOrEmpty;
        var expression = BindExpression(syntax.Expression);

        if (!Scope.TryLookup(name, out var variable) && name != string.Empty)
            diagnostics.ReportUndefinedName(syntax.Identifier.Span, name);
        
        if (variable?.Mutable ?? false)
            diagnostics.ReportCannotAssign(syntax.AssignOperatorToken.Span, name);

        if (expression.Type != variable?.Type)
            diagnostics.ReportCannotConvert(syntax.AssignOperatorToken.Span, expression.Type, variable?.Type);

        return new BoundAssignOperationStatement(variable, syntax.AssignOperatorToken, expression);
    }

    private BoundStatement BindIfStatement(IfStatement syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Boolean);
        var body = BindStatement(syntax.Body);
        
        var elseBody = syntax.ElseClause switch {
            null => null,
            _ => BindStatement(syntax.ElseClause.Body),
        };

        return new BoundIfStatement(condition, body, elseBody);
    }

    private BoundStatement BindWhileStatement(WhileStatement syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Boolean);
        var body = BindStatement(syntax.Body);
        return new BoundWhileStatement(condition, body);
    }

    private BoundStatement BindForStatement(ForStatement syntax) {
        var range = BindExpression(syntax.Range);

        Scope = new BoundScope(Scope);

        var variable = BindVariable(syntax.Identifier, true, TypeSymbol.Int);
        var body = BindStatement(syntax.Body);

        if (Scope.Parent != null)
            Scope = Scope.Parent;

        return new BoundForStatement(variable, range, body);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatement syntax) {
        var expression = BindExpression(syntax.Expression, true);
        return new BoundExpressionStatement(expression);
    }

    private BoundExpression BindExpression(Expression syntax, bool isVoid = false) {
        var result = BindExpressionInternal(syntax);

        if (!isVoid && result.Type == TypeSymbol.Void) {
            diagnostics.ReportVoidExpression(syntax.Span);
            return new BoundErrorExpression();
        }

        return result;
    }

    private BoundExpression BindExpressionInternal(Expression syntax) {
        return syntax.Kind switch {
            SyntaxKind.LiteralExpression        => BindLiteralExpression((LiteralExpression)syntax),
            SyntaxKind.NameExpression           => BindNameExpression((NameExpression)syntax),
            SyntaxKind.AssignmentExpression     => BindAssignmentExpression((AssignmentExpression)syntax),
            SyntaxKind.UnaryExpression          => BindUnaryExpression((UnaryExpression)syntax),
            SyntaxKind.BinaryExpression         => BindBinaryExpression((BinaryExpression)syntax),
            SyntaxKind.RangeExpression          => BindRangeExpression((RangeExpression)syntax),
            SyntaxKind.ParenthesizedExpression  => BindExpression(((ParenthesizedExpression)syntax).Expression),
            SyntaxKind.CallExpression           => BindCallExpression((CallExpression)syntax),
            
            _ => throw new Exception($"Unexpected expression: {syntax.Kind}"),
        };
    }

    private BoundExpression BindCallExpression(CallExpression syntax) {
        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach (var argument in syntax.Arguments) {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        var functions = BuiltinFunctions.All();
        var function = functions.SingleOrDefault(f => f?.Name == syntax.Identifier.Literal);

        if (function is null) {
            diagnostics.ReportUndefinedFunctionCall(syntax.Identifier.Span, syntax.Identifier.LiteralOrEmpty);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function.Parameters.Length) {
            diagnostics.ReportIncorrectArgumentCount(
                syntax.Span, 
                function.Name, 
                function.Parameters.Length, 
                syntax.Arguments.Count
            );
            
            return new BoundErrorExpression();
        }

        for (var i = 0; i < boundArguments.Count; i++) {
            var parameter = function.Parameters[i];
            var argument = boundArguments[i];
            
            if (argument.Type != parameter.Type) {

                diagnostics.ReportIncorrectArgumentType(
                    syntax.Span, 
                    parameter.Name,
                    parameter.Type,
                    argument.Type
                );
                
                return new BoundErrorExpression();
            }
        }

        return new BoundCallExpression(function, boundArguments.ToImmutableArray());
    }

    private BoundExpression BindExpression(Expression syntax, TypeSymbol targetType) {
        var result = BindExpression(syntax);
        
        if (result.Type != targetType) {
            diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
            return new BoundErrorExpression();
        }
        
        return result;
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

        if (token.IsMissing)
            return new BoundErrorExpression();

        if (!Scope.TryLookup(token.LiteralOrEmpty, out var variable) && !token.IsMissing) {
            diagnostics.ReportUndefinedName(token.Span, token.LiteralOrEmpty);
            return new BoundErrorExpression();
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpression syntax) {
        var name = syntax.IdentifierToken.LiteralOrEmpty;
        var expression = BindExpression(syntax.Expression);
        
        if (syntax.IdentifierToken.IsMissing)
            return expression;

        if (!Scope.TryLookup(name, out var variable) && name != string.Empty) {
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

        if (operand.Type.IsError)
            return new BoundErrorExpression();

        var op = UnaryOperator.Bind(token.Kind, operand.Type);
        if (op == null) {
            diagnostics.ReportUndefinedUnaryOperator(token.Span, token.LiteralOrEmpty, operand.Type);
            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(operand, op);
    }

    private BoundExpression BindBinaryExpression(BinaryExpression syntax) {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);
        var token = syntax.OperatorToken;

        if (left.Type.IsError || right.Type.IsError)
            return new BoundErrorExpression();

        var op = BinaryOperator.Bind(token.Kind, left.Type, right.Type);
        
        if (op == null) {
            diagnostics.ReportUndefinedBinaryOperator(token.Span, token.LiteralOrEmpty, left.Type, right.Type);
            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(left, right, op);
    }

    private BoundExpression BindRangeExpression(RangeExpression syntax) {
        var lower = BindExpression(syntax.LowerBound);
        var rangeToken = syntax.RangeToken;
        var upper = BindExpression(syntax.UpperBound);

        if (lower.Type.IsError || upper.Type.IsError)
            return new BoundErrorExpression();

        if (lower.Type != TypeSymbol.Int) {
            diagnostics.ReportNonIntegerRange(syntax.LowerBound.Span);
            return new BoundErrorExpression();
        }

        return new BoundRangeExpression(lower, rangeToken, upper);
    }

    private VariableSymbol BindVariable(SyntaxToken identifier, bool mutable, TypeSymbol type) {
        var name = identifier.LiteralOrEmpty;
        var declare = !identifier.IsMissing;

        var variable = new VariableSymbol(name, mutable, type);
        if (declare && !Scope.TryDeclare(variable))
            diagnostics.ReportVariableRedeclaration(identifier.Span, name);
        
        return variable;
    }

    private readonly Diagnostics diagnostics = [];
}