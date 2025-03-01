using System.Collections.Immutable;
using Sprig.Code.Symbols;
using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal sealed class Binder(BoundScope? parent) {

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnit compilation) {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope);
        
        var statement = binder.BindStatement(compilation.Statement);
        var variables = binder.Scope.Variables;
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        return new BoundGlobalScope(previous, diagnostics, variables, statement);
    }

    public Diagnostics Diagnostics => diagnostics;
    public BoundScope Scope = new(parent);

    private BoundStatement BindStatement(Statement syntax) {
        return syntax.Kind switch {
            SyntaxKind.BlockStatment            => BindBlockStatement((BlockStatement)syntax),
            SyntaxKind.VariableDeclaration      => BindVariableDeclaration((VariableDeclarationStatement)syntax),
            SyntaxKind.ExpressionStatement      => BindExpressionStatement((ExpressionStatement)syntax),
            SyntaxKind.IfStatement              => BindIfStatement((IfStatement)syntax),
            SyntaxKind.WhileStatement           => BindWhileStatement((WhileStatement)syntax),
            SyntaxKind.DoWhileStatement         => BindDoWhileStatement((DoWhileStatement)syntax),
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

    private BoundStatement BindDoWhileStatement(DoWhileStatement syntax) {
        var body = BindStatement(syntax.Body);
        var condition = BindExpression(syntax.Condition, TypeSymbol.Boolean);
        return new BoundDoWhileStatement(body, condition);
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

    private BoundExpression BindExpression(Expression syntax, TypeSymbol targetType) {
        var result = BindExpression(syntax);
        
        if (
            targetType != TypeSymbol.Error && 
            result.Type != TypeSymbol.Error && 
            result.Type != targetType
        ) {
            diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
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

        if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.LiteralOrEmpty) is TypeSymbol type)
            return BindCast(type, syntax.Arguments[0]);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach (var argument in syntax.Arguments) {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        if (!Scope.TryLookupFunction(syntax.Identifier.LiteralOrEmpty, out var function)) {
            diagnostics.ReportUndefinedFunctionCall(syntax.Identifier.Span, syntax.Identifier.LiteralOrEmpty);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function?.Parameters.Length) {
            diagnostics.ReportIncorrectArgumentCount(
                syntax.Span, 
                function?.Name ?? "", 
                function?.Parameters.Length ?? 0, 
                syntax.Arguments.Count
            );
            
            return new BoundErrorExpression();
        }

        for (var i = 0; i < boundArguments.Count; i++) {
            var parameter = function.Parameters[i];
            var argument = boundArguments[i];
            
            if (argument.Type != parameter.Type) {
                diagnostics.ReportIncorrectArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                return new BoundErrorExpression();
            }
        }

        return new BoundCallExpression(function, boundArguments.ToImmutableArray());
    }

    private static BoundExpression BindLiteralExpression(LiteralExpression syntax) {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindNameExpression(NameExpression syntax) {
        var token = syntax.IdentifierToken;

        if (token.IsMissing)
            return new BoundErrorExpression();

        if (!Scope.TryLookupVariable(token.LiteralOrEmpty, out var variable) && !token.IsMissing) {
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

        if (!Scope.TryLookupVariable(name, out var variable) && name != string.Empty) {
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

    private static BoundScope? CreateParentScope(BoundGlobalScope? previous) {
        
        var stack = new Stack<BoundGlobalScope>();
        while (previous != null) {
            stack.Push(previous);
            previous = previous.Previous;
        }

        var parent = CreateRootScope();

        while (stack.Count > 0) {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            
            foreach (var variable in previous.Variables)
                scope.TryDeclareVariable(variable);

            parent = scope;
        }

        return parent;
    }

    private static BoundScope? CreateRootScope() {
        var result = new BoundScope(null);
        
        foreach (var function in BuiltinFunctions.All())
            result.TryDeclareFunction(function);

        return result;
    }

    private BoundExpression BindCast(TypeSymbol type, Expression syntax) {
        var expression = BindExpression(syntax);
        var cast = Casting.Classify(expression.Type, type);

        if (!cast.Exists) {
            diagnostics.ReportCannotConvert(syntax.Span, expression.Type, type);
            return new BoundErrorExpression();
        }

        return new BoundCastExpression(type, expression);
    }

    private VariableSymbol BindVariable(SyntaxToken identifier, bool mutable, TypeSymbol type) {
        var name = identifier.LiteralOrEmpty;
        var declare = !identifier.IsMissing;

        var variable = new VariableSymbol(name, mutable, type);
        if (declare && !Scope.TryDeclareVariable(variable))
            diagnostics.ReportVariableRedeclaration(identifier.Span, name);
        
        return variable;
    }

    private static TypeSymbol? LookupType(string name) => name switch {
        "bool" => TypeSymbol.Boolean,
        "int" => TypeSymbol.Int,
        "string" => TypeSymbol.String,
        _ => null,
    };

    private readonly Diagnostics diagnostics = [];
}