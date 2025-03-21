using System.Collections.Immutable;

namespace Sprig.Codegen.IR;

internal abstract class IR_TreeRewriter {

    public virtual IR_Statement RewriteStatement(IR_Statement node) => node.Kind switch {
        IR_NodeKind.BlockStatement                => RewriteBlockStatement((IR_BlockStatement)node),
        IR_NodeKind.NopStatement                  => RewriteNopStatement((IR_NopStatement)node),
        IR_NodeKind.ExpressionStatement           => RewriteExpressionStatement((IR_ExpressionStatement)node),
        IR_NodeKind.GotoStatement                 => RewriteGotoStatement((IR_GotoStatement)node),
        IR_NodeKind.ConditionalGotoStatement      => RewriteConditionalGotoStatement((IR_ConditionalGotoStatement)node),
        IR_NodeKind.LabelStatement                => RewriteLabelStatement((IR_LabelStatement)node),
        IR_NodeKind.ReturnStatement               => RewriteReturnStatment((IR_ReturnStatment)node),
        IR_NodeKind.IfStatement                   => RewriteIfStatement((IR_IfStatement)node),
        IR_NodeKind.WhileStatement                => RewriteWhileStatement((IR_WhileStatement)node),
        IR_NodeKind.DoWhileStatement              => RewriteDoWhileStatement((IR_DoWhileStatement)node),
        IR_NodeKind.ForStatement                  => RewriteForStatement((IR_ForStatement)node),
        IR_NodeKind.VariableDeclaration  => RewriteVariableDeclarationStatement((IR_VariableDeclaration)node),

        _ => throw new Exception($"Unexpected node: {node.Kind}"),
    };

    protected virtual IR_Statement RewriteNopStatement(IR_NopStatement node) => node;

    protected virtual IR_Statement RewriteGotoStatement(IR_GotoStatement node) => node;

    protected virtual IR_Statement RewriteConditionalGotoStatement(IR_ConditionalGotoStatement node){
        var condition = RewriteExpression(node.Condition);
        if (condition == node.Condition)
            return node;
        
        return new IR_ConditionalGotoStatement(node.Label, condition, node.Jump);
    }

    protected virtual IR_Statement RewriteLabelStatement(IR_LabelStatement node) => node;

    protected virtual IR_Statement RewriteReturnStatment(IR_ReturnStatment node) {
        var expression = node.Expression is null 
            ? null 
            : RewriteExpression(node.Expression);
        
        if (expression == node.Expression)
            return node;

        return new IR_ReturnStatment(expression);
    }

    protected virtual IR_Statement RewriteBlockStatement(IR_BlockStatement node) {
        ImmutableArray<IR_Statement>.Builder? builder = null;

        for (var i = 0; i < node.Statements.Length; i++) {
            var oldStatement = node.Statements[i];
            var newStatement = RewriteStatement(oldStatement);

            if (newStatement != oldStatement && builder is null) {
                builder = ImmutableArray.CreateBuilder<IR_Statement>(node.Statements.Length);
                
                for (var j = 0; j < i; j++) {
                    builder.Add(node.Statements[j]);
                }
            }
            builder?.Add(newStatement);
        }

        return builder is null ? node : new IR_BlockStatement(builder.MoveToImmutable());
    }

    protected virtual IR_Statement RewriteExpressionStatement(IR_ExpressionStatement node) {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new IR_ExpressionStatement(expression);
    }

    protected virtual IR_Statement RewriteIfStatement(IR_IfStatement node) {
        var condition = RewriteExpression(node.Condition);
        var ifStatement = RewriteStatement(node.IfStatement);
        
        IR_Statement? elseStatement = null;
        if (node.ElseStatement != null)
            elseStatement = RewriteStatement(node.ElseStatement);

        return new IR_IfStatement(condition, ifStatement, elseStatement);
    }

    protected virtual IR_Statement RewriteWhileStatement(IR_WhileStatement node) {
        var condition = RewriteExpression(node.Condition);
        var body = RewriteStatement(node.Body);

        if (condition == node.Condition && body == node.Body)
            return node;
        
        return new IR_WhileStatement(condition, body, node.JumpLabel);
    }

    protected virtual IR_Statement RewriteDoWhileStatement(IR_DoWhileStatement node) {
        var body = RewriteStatement(node.Body);
        var condition = RewriteExpression(node.Condition);

        if (body == node.Body && condition == node.Condition)
            return node;

        return new IR_DoWhileStatement(body, condition, node.JumpLabel);
    }

    protected virtual IR_Statement RewriteForStatement(IR_ForStatement node) {
        var lowerBound = RewriteExpression(node.LowerBound);
        var upperBound = RewriteExpression(node.UpperBound);
        var body = RewriteStatement(node.Body);

        if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body)
            return node;

        return new IR_ForStatement(node.Variable, lowerBound, upperBound, body, node.JumpLabel);
    }

    protected virtual IR_Statement RewriteVariableDeclarationStatement(IR_VariableDeclaration node) {
        var initializer = RewriteExpression(node.Initializer);

        if (initializer == node.Initializer)
            return node;

        return new IR_VariableDeclaration(node.Variable, initializer);
    }

    public virtual IR_Expression RewriteExpression(IR_Expression node) => node.Kind switch {
        IR_NodeKind.ErrorExpression           => RewriteErrorExpression((IR_ErrorExpression)node),
        IR_NodeKind.LiteralExpression         => RewriteLiteralExpression((IR_LiteralExpression)node),
        IR_NodeKind.VariableExpression        => RewriteVariableExpression((IR_VariableExpression)node),
        IR_NodeKind.AssignmentExpression      => RewriteAssignmentExpression((IR_AssignmentExpression)node),
        IR_NodeKind.UnaryExpression           => RewriteUnaryExpression((IR_UnaryExpression)node),
        IR_NodeKind.BinaryExpression          => RewriteBinaryExpression((IR_BinaryExpression)node),
        IR_NodeKind.CallExpression            => RewriteCallExpression((IR_CallExpression)node),
        IR_NodeKind.CastExpression            => RewriteCastExpression((IR_CastExpression)node),
        
        _ => throw new Exception($"Unexpected node: {node.Kind}"),
    };

    protected virtual IR_Expression RewriteErrorExpression(IR_ErrorExpression node) => node;

    protected virtual IR_Expression RewriteLiteralExpression(IR_LiteralExpression node) => node;

    protected virtual IR_Expression RewriteVariableExpression(IR_VariableExpression node) => node;

    protected virtual IR_Expression RewriteAssignmentExpression(IR_AssignmentExpression node) {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;
        
        return new IR_AssignmentExpression(node.Variable, expression);
    }

    protected virtual IR_Expression RewriteUnaryExpression(IR_UnaryExpression node) {
        var operand = RewriteExpression(node.Operand);
        if (operand == node.Operand)
            return node;
        
        return new IR_UnaryExpression(node.Operator, operand);
    }

    protected virtual IR_Expression RewriteBinaryExpression(IR_BinaryExpression node) {
        var left = RewriteExpression(node.Left);
        var right = RewriteExpression(node.Right);
        
        if (left == node.Left && right == node.Right)
            return node;
        
        return new IR_BinaryExpression(left, right, node.Operator);
    }

    protected virtual IR_Expression RewriteCallExpression(IR_CallExpression node) {
        ImmutableArray<IR_Expression>.Builder? builder = null;

        for (var i = 0; i < node.Arguments.Length; i++) {
            var oldArgument = node.Arguments[i];
            var newArgument = RewriteExpression(oldArgument);

            if (newArgument != oldArgument && builder is null) {
                builder = ImmutableArray.CreateBuilder<IR_Expression>(node.Arguments.Length);
                
                for (var j = 0; j < i; j++) {
                    builder.Add(node.Arguments[j]);
                }
            }
            builder?.Add(newArgument);
        }

        return builder is null ? node : new IR_CallExpression(node.Function, builder.MoveToImmutable());
    }

    protected virtual IR_Expression RewriteCastExpression(IR_CastExpression node) {
        var expression = RewriteExpression(node.Expression);
        if (expression.Type != node.Type)
            return node;
        
        return new IR_CastExpression(node.Type, expression);
    }
}