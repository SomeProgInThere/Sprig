using System.Collections.Immutable;

namespace Sprig.Codegen.IRGeneration;

internal abstract class IRTreeRewriter {

    public virtual IRStatement RewriteStatement(IRStatement node) => node.Kind switch {
        IRNodeKind.BlockStatement                => RewriteBlockStatement((IRBlockStatement)node),
        IRNodeKind.ExpressionStatement           => RewriteExpressionStatement((IRExpressionStatement)node),
        IRNodeKind.GotoStatement                 => RewriteGotoStatement((IRGotoStatement)node),
        IRNodeKind.ConditionalGotoStatement      => RewriteConditionalGotoStatement((IRConditionalGotoStatement)node),
        IRNodeKind.LabelStatement                => RewriteLabelStatement((IRLabelStatement)node),
        IRNodeKind.ReturnStatement               => RewriteReturnStatment((IRReturnStatment)node),
        IRNodeKind.IfStatement                   => RewriteIfStatement((IRIfStatement)node),
        IRNodeKind.WhileStatement                => RewriteWhileStatement((IRWhileStatement)node),
        IRNodeKind.DoWhileStatement              => RewriteDoWhileStatement((IRDoWhileStatement)node),
        IRNodeKind.ForStatement                  => RewriteForStatement((IRForStatement)node),
        IRNodeKind.VariableDeclaration  => RewriteVariableDeclarationStatement((IRVariableDeclaration)node),

        _ => throw new Exception($"Unexpected node: {node.Kind}"),
    };

    private static IRStatement RewriteGotoStatement(IRGotoStatement node) => node;

    private IRStatement RewriteConditionalGotoStatement(IRConditionalGotoStatement node){
        var condition = RewriteExpression(node.Condition);
        if (condition == node.Condition)
            return node;
        
        return new IRConditionalGotoStatement(node.Label, condition, node.Jump);
    }

    private static IRStatement RewriteLabelStatement(IRLabelStatement node) => node;

    protected virtual IRStatement RewriteReturnStatment(IRReturnStatment node) {
        var expression = node.Expression is null 
            ? null 
            : RewriteExpression(node.Expression);
        
        if (expression == node.Expression)
            return node;

        return new IRReturnStatment(expression);
    }

    protected virtual IRStatement RewriteBlockStatement(IRBlockStatement node) {
        ImmutableArray<IRStatement>.Builder? builder = null;

        for (var i = 0; i < node.Statements.Length; i++) {
            var oldStatement = node.Statements[i];
            var newStatement = RewriteStatement(oldStatement);

            if (newStatement != oldStatement && builder is null) {
                builder = ImmutableArray.CreateBuilder<IRStatement>(node.Statements.Length);
                
                for (var j = 0; j < i; j++) {
                    builder.Add(node.Statements[j]);
                }
            }
            builder?.Add(newStatement);
        }

        return builder is null ? node : new IRBlockStatement(builder.MoveToImmutable());
    }

    protected virtual IRStatement RewriteExpressionStatement(IRExpressionStatement node) {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new IRExpressionStatement(expression);
    }

    protected virtual IRStatement RewriteIfStatement(IRIfStatement node) {
        var condition = RewriteExpression(node.Condition);
        var ifStatement = RewriteStatement(node.IfStatement);
        
        IRStatement? elseStatement = null;
        if (node.ElseStatement != null)
            elseStatement = RewriteStatement(node.ElseStatement);

        return new IRIfStatement(condition, ifStatement, elseStatement);
    }

    protected virtual IRStatement RewriteWhileStatement(IRWhileStatement node) {
        var condition = RewriteExpression(node.Condition);
        var body = RewriteStatement(node.Body);

        if (condition == node.Condition && body == node.Body)
            return node;
        
        return new IRWhileStatement(condition, body, node.JumpLabel);
    }

    protected virtual IRStatement RewriteDoWhileStatement(IRDoWhileStatement node) {
        var body = RewriteStatement(node.Body);
        var condition = RewriteExpression(node.Condition);

        if (body == node.Body && condition == node.Condition)
            return node;

        return new IRDoWhileStatement(body, condition, node.JumpLabel);
    }

    protected virtual IRStatement RewriteForStatement(IRForStatement node) {
        var range = RewriteExpression(node.Range);
        var body = RewriteStatement(node.Body);

        if (range == node.Range && body == node.Body)
            return node;

        return new IRForStatement(node.Variable, range, body, node.JumpLabel);
    }

    protected virtual IRStatement RewriteVariableDeclarationStatement(IRVariableDeclaration node) {
        var initializer = RewriteExpression(node.Initializer);

        if (initializer == node.Initializer)
            return node;

        return new IRVariableDeclaration(node.Variable, initializer);
    }

    public virtual IRExpression RewriteExpression(IRExpression node) => node.Kind switch {
        IRNodeKind.ErrorExpression           => RewriteErrorExpression((IRErrorExpression)node),
        IRNodeKind.LiteralExpression         => RewriteLiteralExpression((IRLiteralExpression)node),
        IRNodeKind.VariableExpression        => RewriteVariableExpression((IRVariableExpression)node),
        IRNodeKind.AssignmentExpression      => RewriteAssignmentExpression((IRAssignmentExpression)node),
        IRNodeKind.UnaryExpression           => RewriteUnaryExpression((IRUnaryExpression)node),
        IRNodeKind.BinaryExpression          => RewriteBinaryExpression((IRBinaryExpression)node),
        IRNodeKind.RangeExpression           => RewriteRangeExpression((IRRangeExpression)node),
        IRNodeKind.CallExpression            => RewriteCallExpression((IRCallExpression)node),
        IRNodeKind.CastExpression            => RewriteCastExpression((IRCastExpression)node),
        
        _ => throw new Exception($"Unexpected node: {node.Kind}"),
    };

    protected virtual IRExpression RewriteErrorExpression(IRErrorExpression node) => node;

    protected virtual IRExpression RewriteLiteralExpression(IRLiteralExpression node) => node;

    protected virtual IRExpression RewriteVariableExpression(IRVariableExpression node) => node;

    protected virtual IRExpression RewriteAssignmentExpression(IRAssignmentExpression node) {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;
        
        return new IRAssignmentExpression(node.Variable, expression);
    }

    protected virtual IRExpression RewriteUnaryExpression(IRUnaryExpression node) {
        var operand = RewriteExpression(node.Operand);
        if (operand == node.Operand)
            return node;
        
        return new IRUnaryExpression(operand, node.Operator);
    }

    protected virtual IRExpression RewriteBinaryExpression(IRBinaryExpression node) {
        var left = RewriteExpression(node.Left);
        var right = RewriteExpression(node.Right);
        
        if (left == node.Left && right == node.Right)
            return node;
        
        return new IRBinaryExpression(left, right, node.Operator);
    }

    protected virtual IRExpression RewriteRangeExpression(IRRangeExpression node) {
        var lower = RewriteExpression(node.Lower);
        var upper = RewriteExpression(node.Upper);

        if (lower == node.Lower && upper == node.Upper)
            return node;

        return new IRRangeExpression(lower, upper);
    }

    protected virtual IRExpression RewriteCallExpression(IRCallExpression node) {
        ImmutableArray<IRExpression>.Builder? builder = null;

        for (var i = 0; i < node.Arguments.Length; i++) {
            var oldArgument = node.Arguments[i];
            var newArgument = RewriteExpression(oldArgument);

            if (newArgument != oldArgument && builder is null) {
                builder = ImmutableArray.CreateBuilder<IRExpression>(node.Arguments.Length);
                
                for (var j = 0; j < i; j++) {
                    builder.Add(node.Arguments[j]);
                }
            }
            builder?.Add(newArgument);
        }

        return builder is null ? node : new IRCallExpression(node.Function, builder.MoveToImmutable());
    }

    protected virtual IRExpression RewriteCastExpression(IRCastExpression node) {
        var expression = RewriteExpression(node.Expression);
        if (expression.Type != node.Type)
            return node;
        
        return new IRCastExpression(node.Type, expression);
    }
}