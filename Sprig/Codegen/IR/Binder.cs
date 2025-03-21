using System.Collections.Immutable;

using Sprig.Codegen.IR.ControlFlow;
using Sprig.Codegen.Text;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IR;

internal sealed class Binder {

    public Binder(LocalScope? parent, FunctionSymbol? function = null) {
        this.parent = parent;
        this.function = function;
        scope = new LocalScope(parent);

        if (function != null) {
            foreach (var parameter in function.Parameters)
                scope.TryDeclareSymbol(parameter);
        }
    }

    public static GlobalScope BindGlobalScope(GlobalScope? previous, ImmutableArray<SyntaxTree> syntaxTrees) {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope);
        
        var functionHeaders = syntaxTrees
            .SelectMany(tree => tree.Root.Members)
            .OfType<FunctionHeader>();

        foreach (var function in functionHeaders)
            binder.BindFunctionHeader(function);

        var globalStatements = syntaxTrees
            .SelectMany(tree => tree.Root.Members)
            .OfType<GlobalStatment>();

        var statments = ImmutableArray.CreateBuilder<IR_Statement>();

        foreach (var globalStatement in globalStatements) {
            var boundStatement = binder.BindGlobalStatement(globalStatement.Statement);
            statments.Add(boundStatement);
        }

        // Check for multiple global statements

        var firstGlobalStatementPerTree = syntaxTrees
            .Select(tree => tree.Root.Members.OfType<GlobalStatment>()
            .FirstOrDefault())
            .Where(statement => statement != null)
            .ToArray();

        if (firstGlobalStatementPerTree.Length > 1) {
            foreach (var globalStatement in firstGlobalStatementPerTree)
                binder.Diagnostics.ReportMultipleGlobalStatements(globalStatement.Location);
        }

        var symbols = binder.scope.Symbols;

        // Check for main function or generate one if not declared

        var mainFunction = symbols
            .OfType<FunctionSymbol>()
            .FirstOrDefault(f => f.Name == "main");

        if (mainFunction != null) {
            if (mainFunction.Type != TypeSymbol.Void || mainFunction.Parameters.Any())
                binder.Diagnostics.ReportIncorrectMainDefinition(mainFunction.Header.Identifier.Location);
        }

        if (globalStatements.Any()) {
            if (mainFunction != null) {
                binder.Diagnostics.ReportMainAlreadyExists(mainFunction.Header.Identifier.Location);

                foreach (var globalStatement in firstGlobalStatementPerTree)
                    binder.Diagnostics.ReportMainAlreadyExists(globalStatement.Location);                
            }
            else {
                mainFunction = new FunctionSymbol("$main", [], TypeSymbol.Void);
            }
        }

        var diagnostics = binder.Diagnostics.ToImmutableArray();
        if (previous != null)
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new GlobalScope(previous, diagnostics, mainFunction, symbols, statments.ToImmutable());
    }

    public static IR_Program BindProgram(IR_Program previous, GlobalScope globalScope) {
        var parentScope = CreateParentScope(globalScope);
        
        var functions = ImmutableDictionary.CreateBuilder<FunctionSymbol, IR_BlockStatement>();
        var diagnostics = ImmutableArray<DiagnosticMessage>.Empty;

        foreach (var symbol in globalScope.Symbols.Where(s => s is FunctionSymbol)) {
            var function = symbol as FunctionSymbol;

            var binder = new Binder(parentScope, function);
            var body = binder.BindStatement(function.Header.Body);
            var loweredBody = Lowerer.Lower(function, body);
            
            if (function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                binder.diagnostics.ReportNotAllPathsReturn(function.Header.Identifier.Location);

            functions.Add(function, loweredBody);
            diagnostics = diagnostics.AddRange(binder.Diagnostics);
        }

        if (globalScope.MainFunction != null && globalScope.Statements.Any()) {
            var body = Lowerer.Lower(globalScope.MainFunction, new IR_BlockStatement(globalScope.Statements));
            functions.Add(globalScope.MainFunction, body);
        }

        return new IR_Program(previous, globalScope.MainFunction, diagnostics, functions.ToImmutable());
    }

    public Diagnostics Diagnostics => diagnostics;
    public LocalScope scope;

    private static LocalScope? CreateParentScope(GlobalScope? previous) {
        var stack = new Stack<GlobalScope>();

        while (previous != null) {
            stack.Push(previous);
            previous = previous.Previous;
        }

        var parent = CreateRootScope();
        
        while (stack.Count > 0) {
            previous = stack.Pop();
            var scope = new LocalScope(parent);

            foreach (var symbols in previous.Symbols)
                scope.TryDeclareSymbol(symbols);

            parent = scope;
        }

        return parent;
    }

    private static LocalScope? CreateRootScope() {
        var result = new LocalScope();
    
        foreach (var function in BuiltinFunctions.All())
            result.TryDeclareSymbol(function);

        return result;
    }

    private void BindFunctionHeader(FunctionHeader syntax) {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var existantParameters = new HashSet<string>();

        foreach(var parameter in syntax.Parameters) {
            var parameterName = parameter.Identifier.Text;
            var parameterType = BindTypeClause(parameter.Type) ?? TypeSymbol.Error;
            
            if (!existantParameters.Add(parameterName))
                diagnostics.ReportParameterAlreadyExists(parameter.Location, parameterName);

            else {
                var parameterSymbol = new ParameterSymbol(parameterName, parameterType, parameters.Count);
                parameters.Add(parameterSymbol);
            }
        }

        var type = BindTypeClause(syntax.ReturnType) ?? TypeSymbol.Void;
        var function = new FunctionSymbol(
            syntax.Identifier.Text, 
            parameters.ToImmutable(), 
            type, 
            syntax
        );
        
        if (!scope.TryDeclareSymbol(function))
            diagnostics.ReportSymbolAlreadyExists(syntax.Identifier.Location, function.Name);
    }

    private TypeSymbol? BindTypeClause(TypeClause clause) {
        TypeSymbol? type = null;
        if (clause != null) {
            var typeIdentifier = clause.Identifier;
            type = LookupType(typeIdentifier.Text);

            if (type is null)
                diagnostics.ReportUndefinedType(typeIdentifier.Location, typeIdentifier.Text);
        }

        return type;
    }
    
    private IR_Statement BindGlobalStatement(Statement syntax) {
        var boundStatement = BindStatement(syntax);
        if (boundStatement is IR_ExpressionStatement es && !AllowExpression(es))
            diagnostics.ReportInvalidExpressionStatement(syntax.Location);

        return boundStatement;    
    }

    private IR_Statement BindStatement(Statement syntax) {
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

    private static bool AllowExpression(IR_ExpressionStatement statement) {
        var condition = 
            statement.Expression.Kind == IR_NodeKind.ErrorExpression ||
            statement.Expression.Kind == IR_NodeKind.AssignmentExpression ||
            statement.Expression.Kind == IR_NodeKind.CallExpression;
        
        return condition;
    }

    private IR_BlockStatement BindBlockStatement(BlockStatement syntax) {
        var statements = ImmutableArray.CreateBuilder<IR_Statement>();
        scope = new LocalScope(scope);

        for (var i = 0; i < syntax.Statements.Length; i++) {
            var boundStatement = BindStatement(syntax.Statements[i]);

            if (boundStatement is IR_ExpressionStatement statement) {
                if (!AllowExpression(statement))
                    diagnostics.ReportInvalidExpressionStatement(syntax.Statements[i].Location);
            }

            statements.Add(boundStatement);
        }

        scope = scope.Parent ?? new LocalScope(parent);
        return new IR_BlockStatement(statements.ToImmutable());
    }

    private IR_VariableDeclaration BindVariableDeclaration(VariableDeclarationStatement syntax) {
        var mutable = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        TypeSymbol? explicitType = null;
        
        if (syntax.TypeClause != null) {
            var typeIdentifier = syntax.TypeClause?.Identifier;
            explicitType = LookupType(typeIdentifier.Text);

            if (explicitType is null)
                diagnostics.ReportUndefinedType(typeIdentifier.Location, typeIdentifier.Text);
        }

        var initializer = BindExpression(syntax.Initializer);

        var variableType = explicitType ?? initializer.Type;
        var castInitializer = BindCast(syntax.Initializer.Location, initializer, variableType);
        var variable = BindVariableDeclaration(syntax.Identifier, mutable, variableType, initializer.ConstantValue);

        return new IR_VariableDeclaration(variable, castInitializer);
    }

    private IR_IfStatement BindIfStatement(IfStatement syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Boolean);
        var body = BindStatement(syntax.Body);
        
        var elseBody = syntax.ElseClause switch {
            null => null,
            _ => BindStatement(syntax.ElseClause.Body),
        };

        return new IR_IfStatement(condition, body, elseBody);
    }

    private IR_WhileStatement BindWhileStatement(WhileStatement syntax) {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Boolean);
        var body = BindLoopBody(syntax.Body, out var jumpLabel);

        return new IR_WhileStatement(condition, body, jumpLabel);
    }

    private IR_DoWhileStatement BindDoWhileStatement(DoWhileStatement syntax) {
        var body = BindLoopBody(syntax.Body, out var jumpLabel);
        var condition = BindExpression(syntax.Condition, TypeSymbol.Boolean);
        
        return new IR_DoWhileStatement(body, condition, jumpLabel);
    }

    private IR_ForStatement BindForStatement(ForStatement syntax) {
        var range = BindExpression(syntax.Range);
        scope = new LocalScope(scope);

        var variable = BindVariableDeclaration(syntax.Identifier, mutable: true, TypeSymbol.Int32);
        var body = BindLoopBody(syntax.Body, out var jumpLabel);

        if (scope.Parent != null)
            scope = scope.Parent;

        return new IR_ForStatement(variable, range, body, jumpLabel);
    }

    private IR_Statement BindBreakStatement(BreakStatement syntax) {
        if (loopJumps.Count < 0) {
            diagnostics.ReportInvalidJump(syntax.BreakKeyword.Location, syntax.BreakKeyword.Text);
            return BindErrorStatement();
        }

        var breakLabel = loopJumps.Peek().BrakeLabel;
        return new IR_GotoStatement(breakLabel);
    }

    private IR_Statement BindContinueStatement(ContinueStatement syntax) {
        if (loopJumps.Count < 0) {
            diagnostics.ReportInvalidJump(syntax.ContinueKeyword.Location, syntax.ContinueKeyword.Text);
            return BindErrorStatement();
        }

        var continueLabel = loopJumps.Peek().ContinueLabel;
        return new IR_GotoStatement(continueLabel);
    }

    private IR_ReturnStatment BindReturnStatement(ReturnStatement syntax) {
        var expression = syntax.Expression == null 
            ? null 
            : BindExpression(syntax.Expression);
        
        if (function is null) {
            if (expression != null)
                diagnostics.ReportInvalidReturnGlobalStatement(syntax.Expression.Location);
        }
        else {
            if (function.Type == TypeSymbol.Void) {
                if (expression != null)
                    diagnostics.ReportInvalidReturnExpression(syntax.Expression.Location, function.Name);
            }
            else {
                if (expression is null)
                    diagnostics.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, function.Name, function.Type);
                else
                    expression = BindCast(syntax.Expression.Location, expression, function.Type);
            }
        }

        return new IR_ReturnStatment(expression);
    }

    private IR_ExpressionStatement BindExpressionStatement(ExpressionStatement syntax) {
        var expression = BindExpression(syntax.Expression, voidExpression: true);
        return new IR_ExpressionStatement(expression);
    }

    private IR_Statement BindLoopBody(Statement body, out JumpLabel jumpLabel) {
        labelCounter++;

        var breakLabel = new LabelSymbol($"break{labelCounter}");
        var continueLabel = new LabelSymbol($"continue{labelCounter}");

        jumpLabel = new JumpLabel(breakLabel, continueLabel);
        
        loopJumps.Push(jumpLabel);
        var boundBody = BindStatement(body);
        loopJumps.Pop();

        return boundBody;
    }

    private static IR_ExpressionStatement BindErrorStatement() => new(new IR_ErrorExpression());

    private IR_Expression BindExpression(Expression syntax, bool voidExpression = false) {
        var result = BindExpressionInternal(syntax);

        if (!voidExpression && result.Type == TypeSymbol.Void) {
            diagnostics.ReportVoidExpression(syntax.Location);
            return new IR_ErrorExpression();
        }

        return result;
    }

    private IR_Expression BindExpression(Expression syntax, TypeSymbol targetType) {
        return BindCast(syntax, targetType);
    }

    private IR_Expression BindExpressionInternal(Expression syntax) {
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

    private IR_Expression BindCallExpression(CallExpression syntax) {
        if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            return BindCast(syntax.Arguments[0], type, allowExplicit: true);

        var boundArguments = ImmutableArray.CreateBuilder<IR_Expression>();

        foreach (var argument in syntax.Arguments) {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        var symbol = scope.TryLookupSymbol(syntax.Identifier.Text);
        if (symbol is null) {
            diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new IR_ErrorExpression();
        }

        if (symbol is not FunctionSymbol function) {
            diagnostics.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new IR_ErrorExpression();
        }

        if (syntax.Arguments.Count != function?.Parameters.Length) {
            TextLocation location;

            if (syntax.Arguments.Count > function.Parameters.Length) {
                SyntaxNode? firstExcessNode = function.Parameters.Length > 0 
                    ? syntax.Arguments.Seperator(function.Parameters.Length - 1)
                    : syntax.Arguments[0];
                
                var lastExcessArgument = syntax.Arguments[^1];
                var span = TextSpan.CreateFromBounds(firstExcessNode.Span.Start, lastExcessArgument.Span.End);

                location = new TextLocation(syntax.SyntaxTree.Source, span);
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
            
            return new IR_ErrorExpression();
        }

        bool hasErrors = false;
        for (var i = 0; i < boundArguments.Count; i++) {
            var argument = boundArguments[i];
            var parameter = function.Parameters[i];
            boundArguments[i] = BindCast(syntax.Arguments[i].Location, argument, parameter.Type);
        }

        if (hasErrors)
            return new IR_ErrorExpression();

        return new IR_CallExpression(function, boundArguments.ToImmutableArray());
    }

    private static IR_Expression BindLiteralExpression(LiteralExpression syntax) {
        var value = syntax.Value;
        return new IR_LiteralExpression(value);
    }

    private IR_Expression BindNameExpression(NameExpression syntax) {
        var identifier = syntax.Identifier;

        if (identifier.IsMissing)
            return new IR_ErrorExpression();
        
        var variable = BindVariableReference(identifier.Text, identifier.Location);
        if (variable is null)
            return new IR_ErrorExpression();

        return new IR_VariableExpression(variable);
    }

    private IR_Expression BindAssignmentExpression(AssignmentExpression syntax) {
        var name = syntax.Identifier.Text;
        var expression = BindExpression(syntax.Expression);
        
        if (syntax.Identifier.IsMissing)
            return expression;

        var variable = BindVariableReference(name, syntax.Identifier.Location);
        if (variable is null)
            return new IR_ErrorExpression();

        if (variable?.Mutable ?? false)
            diagnostics.ReportCannotAssign(syntax.EqualsToken.Location, name);

        var castExpression = BindCast(syntax.Expression.Location, expression, variable.Type);

        return new IR_AssignmentExpression(variable, castExpression);
    }

    private IR_Expression BindUnaryExpression(UnaryExpression syntax) {
        var operand = BindExpression(syntax.Operand);
        var token = syntax.OperatorToken;

        if (operand.Type.IsError)
            return new IR_ErrorExpression();

        var op = IR_UnaryOperator.Bind(token.Kind, operand.Type);
        if (op == null) {
            diagnostics.ReportUndefinedUnaryOperator(token.Location, token.Text, operand.Type);
            return new IR_ErrorExpression();
        }

        return new IR_UnaryExpression(op, operand);
    }

    private IR_Expression BindBinaryExpression(BinaryExpression syntax) {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);
        var token = syntax.OperatorToken;

        if (left.Type.IsError || right.Type.IsError)
            return new IR_ErrorExpression();

        var op = IR_BinaryOperator.Bind(token.Kind, left.Type, right.Type);
        
        if (op == null) {
            diagnostics.ReportUndefinedBinaryOperator(token.Location, token.Text, left.Type, right.Type);
            return new IR_ErrorExpression();
        }

        return new IR_BinaryExpression(left, right, op);
    }

    private IR_Expression BindRangeExpression(RangeExpression syntax) {
        var lower = BindExpression(syntax.Lower);
        var upper = BindExpression(syntax.Upper);

        if (lower.Type.IsError || upper.Type.IsError)
            return new IR_ErrorExpression();

        if (lower.Type != TypeSymbol.Int32) {
            diagnostics.ReportNonIntegerRange(syntax.Lower.Location);
            return new IR_ErrorExpression();
        }

        return new IR_RangeExpression(lower, upper);
    }

    private IR_Expression BindCast(Expression syntax, TypeSymbol type, bool allowExplicit = false) {
        var expression = BindExpression(syntax);
        return BindCast(syntax.Location, expression, type, allowExplicit);
    }

    private IR_Expression BindCast(TextLocation location, IR_Expression expression, TypeSymbol type, bool allowExplicit = false) {
        var cast = Casting.TypeOf(expression.Type, type);
        
        if (!cast.Exists) {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                diagnostics.ReportCannotConvert(location, expression.Type, type);

            return new IR_ErrorExpression();
        }

        if (!allowExplicit && cast.IsExplicit)
            diagnostics.ReportCannotConvert(location, expression.Type, type, reportCastExisits: true);

        if (cast.IsIdentity)
            return expression;

        return new IR_CastExpression(type, expression);
    }

    private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool mutable, TypeSymbol type, IR_Constant? constant = null) {
        var name = identifier.Text;
        var exists = !identifier.IsMissing;

        var scope = function is null ? VariableScope.Global : VariableScope.Local; 
        var variable = new VariableSymbol(name, mutable, type, scope, constant);
        
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
        "bool"      => TypeSymbol.Boolean,
        "int"       => TypeSymbol.Int32,
        "str"       => TypeSymbol.String,
        "decimal"   => TypeSymbol.Double,
        "any"       => TypeSymbol.Any,
        _ => null,
    };

    private readonly Diagnostics diagnostics = [];
    private readonly Stack<JumpLabel> loopJumps = [];

    private readonly LocalScope? parent;
    private readonly FunctionSymbol? function;
    private int labelCounter;
}