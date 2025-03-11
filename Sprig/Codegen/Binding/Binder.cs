using System.Collections.Immutable;
using Sprig.Codegen.Binding.ControlFlow;
using Sprig.Codegen.Lowering;
using Sprig.Codegen.Source;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Binding;

internal sealed class Binder {

    public Binder(BoundScope? parent, FunctionSymbol? function) {
        this.parent = parent;
        this.function = function;
        scope = new BoundScope(parent);

        if (function != null) {
            foreach (var parameter in function.Parameters)
                scope.TryDeclareSymbol(parameter);
        }
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnit syntax) {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope, null);
        
        foreach (var function in syntax.Members.OfType<FunctionHeader>())
            binder.BindFunctionHeader(function);

        var statmentBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach (var globalStatement in syntax.Members.OfType<GlobalStatment>()) {
            var boundStatement = binder.BindStatement(globalStatement.Statement);
            statmentBuilder.Add(boundStatement);
        }

        var statement = new BoundBlockStatement(statmentBuilder.ToImmutable());
        var symbols = binder.scope.Symbols;
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        if (previous != null)
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new BoundGlobalScope(previous, diagnostics, symbols, statement);
    }

    public static BoundProgram BindProgram(BoundGlobalScope globalScope) {
        var parentScope = CreateParentScope(globalScope);
        var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = ImmutableArray<DiagnosticMessage>.Empty;

        var scope = globalScope;
        while (scope != null) {
            foreach (var symbol in scope.Symbols.Where(s => s is FunctionSymbol)) {
                var function = symbol as FunctionSymbol;
                
                var binder = new Binder(parentScope, function);
                var body = binder.BindStatement(function.Header.Body);
                
                var loweredBody = Lowerer.Lower(body);
                if (function.ReturnType != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    binder.diagnostics.ReportNotAllPathsReturn(function.Header.Identifier.Location);

                functionBodies.Add(function, loweredBody);
                diagnostics = diagnostics.AddRange(binder.Diagnostics);
            }

            scope = scope.Previous;
        }

        return new BoundProgram(globalScope, diagnostics, functionBodies.ToImmutable());
    }

    public Diagnostics Diagnostics => diagnostics;
    public BoundScope scope;

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

            foreach (var symbols in previous.Symbols)
                scope.TryDeclareSymbol(symbols);

            parent = scope;
        }

        return parent;
    }

    private static BoundScope? CreateRootScope() {
        var result = new BoundScope(null);
    
        foreach (var function in BuiltinFunctions.All())
            result.TryDeclareSymbol(function);

        return result;
    }

    private void BindFunctionHeader(FunctionHeader syntax) {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var existantParameters = new HashSet<string>();

        foreach(var parameter in syntax.Parameters) {
            var parameterName = parameter.Identifier.Literal;
            var parameterType = BindTypeClause(parameter.Type) ?? TypeSymbol.Error;
            
            if (!existantParameters.Add(parameterName))
                diagnostics.ReportParameterAlreadyExists(parameter.Location, parameterName);

            else {
                var parameterSymbol = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(parameterSymbol);
            }
        }

        var type = BindTypeClause(syntax.ReturnType) ?? TypeSymbol.Void;
        var function = new FunctionSymbol(syntax.Identifier.Literal, parameters.ToImmutable(), type, syntax);
        
        if (!scope.TryDeclareSymbol(function))
            diagnostics.ReportSymbolAlreadyExists(syntax.Identifier.Location, function.Name);
    }

    private TypeSymbol? BindTypeClause(TypeClause clause) {
        TypeSymbol? type = null;
        if (clause != null) {
            var typeIdentifier = clause.Identifier;
            type = LookupType(typeIdentifier.Literal);

            if (type is null)
                diagnostics.ReportUndefinedType(typeIdentifier.Location, typeIdentifier.Literal);
        }

        return type;
    }

    private BoundStatement BindStatement(Statement syntax) {
        return syntax.Kind switch {
            SyntaxKind.BlockStatment            => BindBlockStatement((BlockStatement)syntax),
            SyntaxKind.VariableDeclaration      => BindVariableDeclaration((VariableDeclarationStatement)syntax),
            SyntaxKind.ExpressionStatement      => BindExpressionStatement((ExpressionStatement)syntax),
            SyntaxKind.IfStatement              => BindIfStatement((IfStatement)syntax),
            SyntaxKind.WhileStatement           => BindWhileStatement((WhileStatement)syntax),
            SyntaxKind.DoWhileStatement         => BindDoWhileStatement((DoWhileStatement)syntax),
            SyntaxKind.ForStatement             => BindForStatement((ForStatement)syntax),
            SyntaxKind.BreakStatement           => BindBreakStatement((BreakStatement)syntax),
            SyntaxKind.ContinueStatement        => BindContinueStatement((ContinueStatement)syntax),
            SyntaxKind.ReturnStatement          => BindReturnStatement((ReturnStatement)syntax),

            _ => throw new Exception($"Unexpected statement: {syntax.Kind}"),
        };
    }

    private BoundStatement BindBlockStatement(BlockStatement syntax) {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        scope = new BoundScope(scope);

        foreach (var statement in syntax.Statements) {
            var boundStatement = BindStatement(statement);
            statements.Add(boundStatement);
        }

        scope = scope.Parent ?? new BoundScope(parent);
        return new BoundBlockStatement(statements.ToImmutable());
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationStatement syntax) {
        var mutable = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        TypeSymbol? explicitType = null;
        
        if (syntax.TypeClause != null) {
            var typeIdentifier = syntax.TypeClause?.Identifier;
            explicitType = LookupType(typeIdentifier.Literal);

            if (explicitType is null)
                diagnostics.ReportUndefinedType(typeIdentifier.Location, typeIdentifier.Literal);
        }

        var initializer = BindExpression(syntax.Initializer);

        var variableType = explicitType ?? initializer.Type;
        var castInitializer = BindCast(syntax.Initializer.Location, initializer, variableType);
        var variable = BindVariableDeclaration(syntax.Identifier, mutable, variableType);

        return new BoundVariableDeclaration(variable, castInitializer);
    }

    private BoundStatement BindIfStatement(IfStatement syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindStatement(syntax.Body);
        
        var elseBody = syntax.ElseClause switch {
            null => null,
            _ => BindStatement(syntax.ElseClause.Body),
        };

        return new BoundIfStatement(condition, body, elseBody);
    }

    private BoundStatement BindWhileStatement(WhileStatement syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindLoopBody(syntax.Body, out var jumpLabel);
        return new BoundWhileStatement(condition, body, jumpLabel);
    }

    private BoundStatement BindDoWhileStatement(DoWhileStatement syntax) {
        var body = BindLoopBody(syntax.Body, out var jumpLabel);
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        return new BoundDoWhileStatement(body, condition, jumpLabel);
    }

    private BoundStatement BindForStatement(ForStatement syntax) {
        var range = BindExpression(syntax.Range);

        scope = new BoundScope(scope);

        var variable = BindVariableDeclaration(syntax.Identifier, true, TypeSymbol.Int);
        var body = BindLoopBody(syntax.Body, out var jumpLabel);

        if (scope.Parent != null)
            scope = scope.Parent;

        return new BoundForStatement(variable, range, body, jumpLabel);
    }

    private BoundStatement BindBreakStatement(BreakStatement syntax) {
        if (loopJumps.Count < 0) {
            diagnostics.ReportInvalidJump(syntax.BreakKeyword.Location, syntax.BreakKeyword.Literal);
            return BindErrorStatement();
        }

        var breakLabel = loopJumps.Peek().BrakeLabel;
        return new BoundGotoStatement(breakLabel);
    }

    private BoundStatement BindContinueStatement(ContinueStatement syntax) {
        if (loopJumps.Count < 0) {
            diagnostics.ReportInvalidJump(syntax.ContinueKeyword.Location, syntax.ContinueKeyword.Literal);
            return BindErrorStatement();
        }

        var continueLabel = loopJumps.Peek().ContinueLabel;
        return new BoundGotoStatement(continueLabel);
    }

    private BoundStatement BindReturnStatement(ReturnStatement syntax) {
        var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);
        
        if (function is null)
            diagnostics.ReportInvalidReturn(syntax.ReturnKeyword.Location);

        else {
            if (function.ReturnType == TypeSymbol.Void) {
                if (expression != null)
                    diagnostics.ReportInvalidReturnExpression(syntax.Expression.Location, function.Name);
            }
            else {
                if (expression is null)
                    diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, function.Name, function.ReturnType);
                
                expression = BindCast(syntax.Expression.Location, expression, function.ReturnType);
            }
        }

        return new BoundReturnStatment(expression);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatement syntax) {
        var expression = BindExpression(syntax.Expression, true);
        return new BoundExpressionStatement(expression);
    }

    private BoundStatement BindLoopBody(Statement body, out JumpLabel jumpLabel) {
        labelCounter++;

        var breakLabel = new LabelSymbol($"break{labelCounter}");
        var continueLabel = new LabelSymbol($"continue{labelCounter}");

        jumpLabel = new JumpLabel(breakLabel, continueLabel);
        
        loopJumps.Push(jumpLabel);
        var boundBody = BindStatement(body);
        loopJumps.Pop();

        return boundBody;
    }

    private static BoundStatement BindErrorStatement() 
        => new BoundExpressionStatement(new BoundErrorExpression());

    private BoundExpression BindExpression(Expression syntax, bool isVoid = false) {
        var result = BindExpressionInternal(syntax);

        if (!isVoid && result.Type == TypeSymbol.Void) {
            diagnostics.ReportVoidExpression(syntax.Location);
            return new BoundErrorExpression();
        }

        return result;
    }

    private BoundExpression BindExpression(Expression syntax, TypeSymbol targetType) {
        return BindCast(syntax, targetType);
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
        if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Literal) is TypeSymbol type)
            return BindCast(syntax.Arguments[0], type, true);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach (var argument in syntax.Arguments) {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        var symbol = scope.TryLookupSymbol(syntax.Identifier.Literal);
        if (symbol is null) {
            diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Literal);
            return new BoundErrorExpression();
        }

        if (symbol is not FunctionSymbol function) {
            diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Literal);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function?.Parameters.Length) {
            TextLocation location;

            if (syntax.Arguments.Count > function.Parameters.Length) {
                SyntaxNode? firstExcessNode = function.Parameters.Length > 0 
                    ? syntax.Arguments.Seperator(function.Parameters.Length - 1)
                    : syntax.Arguments[0];
                
                var lastExcessArgument = syntax.Arguments[^1];
                var span = TextSpan.CreateFromBounds(firstExcessNode.Span.Start, lastExcessArgument.Span.End);

                location = new TextLocation(syntax.SyntaxTree.SourceText, span);
            }
            else {
                location = syntax.ClosedParenthesisToken.Location;
            }
            
            diagnostics.ReportIncorrectArgumentCount(
                location, 
                function?.Name ?? "", 
                function?.Parameters.Length ?? 0, 
                syntax.Arguments.Count
            );
            
            return new BoundErrorExpression();
        }

        bool hasErrors = false;
        for (var i = 0; i < boundArguments.Count; i++) {
            var parameter = function.Parameters[i];
            var argument = boundArguments[i];
            
            if (argument.Type != parameter.Type) {
                if (argument.Type != TypeSymbol.Error)

                    diagnostics.ReportIncorrectArgumentType(
                        syntax.Arguments[i].Location, 
                        parameter.Name, 
                        parameter.Type, 
                        argument.Type
                    );
                
                hasErrors = true;
            }
        }

        if (hasErrors)
            return new BoundErrorExpression();

        return new BoundCallExpression(function, boundArguments.ToImmutableArray());
    }

    private static BoundExpression BindLiteralExpression(LiteralExpression syntax) {
        var value = syntax.Value ?? int.MinValue;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindNameExpression(NameExpression syntax) {
        var identifier = syntax.Identifier;

        if (identifier.IsMissing)
            return new BoundErrorExpression();
        
        var variable = BindVariableReference(identifier.Literal, identifier.Location);
        if (variable is null)
            return new BoundErrorExpression();

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpression syntax) {
        var name = syntax.Identifier.Literal;
        var expression = BindExpression(syntax.Expression);
        
        if (syntax.Identifier.IsMissing)
            return expression;

        var variable = BindVariableReference(name, syntax.Identifier.Location);
        if (variable is null)
            return new BoundErrorExpression();

        if (variable?.Mutable ?? false)
            diagnostics.ReportCannotAssign(syntax.EqualsToken.Location, name);

        var castExpression = BindCast(syntax.Expression.Location, expression, variable.Type);

        return new BoundAssignmentExpression(variable, castExpression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpression syntax) {
        var operand = BindExpression(syntax.Operand);
        var token = syntax.OperatorToken;

        if (operand.Type.IsError)
            return new BoundErrorExpression();

        var op = BoundUnaryOperator.Bind(token.Kind, operand.Type);
        if (op == null) {
            diagnostics.ReportUndefinedUnaryOperator(token.Location, token.Literal, operand.Type);
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

        var op = BoundBinaryOperator.Bind(token.Kind, left.Type, right.Type);
        
        if (op == null) {
            diagnostics.ReportUndefinedBinaryOperator(token.Location, token.Literal, left.Type, right.Type);
            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(left, right, op);
    }

    private BoundExpression BindRangeExpression(RangeExpression syntax) {
        var lower = BindExpression(syntax.LowerBound);
        var upper = BindExpression(syntax.UpperBound);

        if (lower.Type.IsError || upper.Type.IsError)
            return new BoundErrorExpression();

        if (lower.Type != TypeSymbol.Int) {
            diagnostics.ReportNonIntegerRange(syntax.LowerBound.Location);
            return new BoundErrorExpression();
        }

        return new BoundRangeExpression(lower, upper);
    }

    private BoundExpression BindCast(Expression syntax, TypeSymbol type, bool allowExplicit = false) {
        var expression = BindExpression(syntax);
        return BindCast(syntax.Location, expression, type, allowExplicit);
    }

    private BoundExpression BindCast(TextLocation location, BoundExpression expression, TypeSymbol type, bool allowExplicit = false) {
        var cast = Casting.TypeOf(expression.Type, type);
        
        if (!cast.Exists) {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                diagnostics.ReportCannotConvert(location, expression.Type, type);

            return new BoundErrorExpression();
        }

        if (!allowExplicit && cast.IsExplicit)
            diagnostics.ReportCannotConvert(location, expression.Type, type, true);

        if (cast.IsIdentity)
            return expression;

        return new BoundCastExpression(type, expression);
    }

    private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool mutable, TypeSymbol type) {
        var name = identifier.Literal;
        var exists = !identifier.IsMissing;

        var scope = function is null ? VariableScope.Global : VariableScope.Local; 
        var variable = new VariableSymbol(name, mutable, type, scope);
        if (exists && !this.scope.TryDeclareSymbol(variable))
            diagnostics.ReportVariableRedeclaration(identifier.Location, name);
        
        return variable;
    }

    private VariableSymbol? BindVariableReference(string name, TextLocation location) {
        switch (scope.TryLookupSymbol(name)) {
            case VariableSymbol variable:
                return variable;

            case null:
                diagnostics.ReportUndefinedVariable(location, name);
                return null;

            default:
                diagnostics.ReportNotAVariable(location, name);
                return null;
        }
    }

    private static TypeSymbol? LookupType(string name) => name switch {
        "bool"      => TypeSymbol.Bool,
        "int"       => TypeSymbol.Int,
        "string"    => TypeSymbol.String,
        "float"     => TypeSymbol.Float,
        _ => null,
    };

    private readonly Diagnostics diagnostics = [];
    private readonly Stack<JumpLabel> loopJumps = [];

    private readonly BoundScope? parent;
    private readonly FunctionSymbol? function;
    private int labelCounter;
}