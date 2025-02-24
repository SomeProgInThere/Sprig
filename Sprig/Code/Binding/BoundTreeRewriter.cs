
using System.Collections.Immutable;

namespace Sprig.Code.Binding;

internal abstract class BoundTreeRewriter {

    public virtual BoundStatement RewriteStatement(BoundStatement node) => node.Kind switch {
        BoundNodeKind.BlockStatement                => RewriteBlockStatement((BoundBlockStatement)node),
        BoundNodeKind.ExpressionStatement           => RewriteExpressionStatement((BoundExpressionStatement)node),
        BoundNodeKind.GotoStatement                 => RewriteGotoStatement((BoundGotoStatement)node),
        BoundNodeKind.ConditionalGotoStatement      => RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node),
        BoundNodeKind.LabelStatement                => RewriteLabelStatement((BoundLableStatement)node),
        BoundNodeKind.IfStatement                   => RewriteIfStatement((BoundIfStatement)node),
        BoundNodeKind.WhileStatement                => RewriteWhileStatement((BoundWhileStatement)node),
        BoundNodeKind.ForStatement                  => RewriteForStatement((BoundForStatement)node),
        BoundNodeKind.VariableDeclarationStatement  => RewriteVariableDeclarationStatement((BoundVariableDeclarationStatement)node),
        BoundNodeKind.AssignOperationStatement      => RewriteAssignOperationStatement((BoundAssignOperationStatement)node),

        _ => throw new Exception($"Unexpected node: {node.Kind}"),
    };

    private static BoundStatement RewriteGotoStatement(BoundGotoStatement node) {
        return node;
    }

    private BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node){
        var condition = RewriteExpression(node.Condition);
        if (condition == node.Condition)
            return node;
        
        return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfFalse);
    }
    
    private static BoundStatement RewriteLabelStatement(BoundLableStatement node) {
        return node;
    }

    protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node) {
        ImmutableArray<BoundStatement>.Builder? builder = null;

        for (var i = 0; i < node.Statements.Length; i++) {
            var oldStatement = node.Statements[i];
            var newStatement = RewriteStatement(oldStatement);

            if (newStatement != oldStatement && builder is null) {
                builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);
                for (var j = 0; j < i; j++) {
                    builder.Add(node.Statements[j]);
                }
            }
            builder?.Add(newStatement);
        }

        return builder is null ? node : new BoundBlockStatement(builder.MoveToImmutable());
    }

    protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node) {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;

        return new BoundExpressionStatement(expression);
    }

    protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node) {
        var condition = RewriteExpression(node.Condition);
        var ifStatement = RewriteStatement(node.IfStatement);
        
        BoundStatement? elseStatement = null;
        if (node.ElseStatement != null)
            elseStatement = RewriteStatement(node.ElseStatement);

        return new BoundIfStatement(condition, ifStatement, elseStatement);
    }

    protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node) {
        var condition = RewriteExpression(node.Condition);
        var body = RewriteStatement(node.Body);

        if (condition == node.Condition && body == node.Body)
            return node;
        
        return new BoundWhileStatement(condition, body);
    }

    protected virtual BoundStatement RewriteForStatement(BoundForStatement node) {
        var range = RewriteExpression(node.Range);
        var body = RewriteStatement(node.Body);

        if (range == node.Range && body == node.Body)
            return node;

        return new BoundForStatement(node.Variable, range, body);
    }

    protected virtual BoundStatement RewriteVariableDeclarationStatement(BoundVariableDeclarationStatement node) {
        var initializer = RewriteExpression(node.Initializer);

        if (initializer == node.Initializer)
            return node;

        return new BoundVariableDeclarationStatement(node.Variable, initializer);
    }

    protected virtual BoundStatement RewriteAssignOperationStatement(BoundAssignOperationStatement node) {
        var expression = RewriteExpression(node.Expression);

        if (expression == node.Expression)
            return node;

        return new BoundAssignOperationStatement(node.Variable, node.AssignOperatorToken, expression);
    }

    public virtual BoundExpression RewriteExpression(BoundExpression node) => node.Kind switch {
        BoundNodeKind.LiteralExpression       => RewriteLiteralExpression((BoundLiteralExpression)node),
        BoundNodeKind.VariableExpression      => RewriteVariableExpression((BoundVariableExpression)node),
        BoundNodeKind.AssignmentExpression    => RewriteAssignmentExpression((BoundAssignmentExpression)node),
        BoundNodeKind.UnaryExpression         => RewriteUnaryExpression((BoundUnaryExpression)node),
        BoundNodeKind.BinaryExpression        => RewriteBinaryExpression((BoundBinaryExpression)node),
        BoundNodeKind.RangeExpression         => RewriteRangeExpression((BoundRangeExpression)node),   
        
        _ => throw new Exception($"Unexpected node: {node.Kind}"),
    };

    protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node) => node;

    protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node) => node;

    protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node) {
        var expression = RewriteExpression(node.Expression);
        if (expression == node.Expression)
            return node;
        
        return new BoundAssignmentExpression(node.Variable, expression);
    }

    protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node) {
        var operand = RewriteExpression(node.Operand);
        if (operand == node.Operand)
            return node;
        
        return new BoundUnaryExpression(operand, node.Operator);
    }

    protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node) {
        var left = RewriteExpression(node.Left);
        var right = RewriteExpression(node.Right);
        
        if (left == node.Left && right == node.Right)
            return node;
        
        return new BoundBinaryExpression(left, right, node.Operator);
    }

    protected virtual BoundExpression RewriteRangeExpression(BoundRangeExpression node) {
        var lower = RewriteExpression(node.Lower);
        var upper = RewriteExpression(node.Upper);

        if (lower == node.Lower && upper == node.Upper)
            return node;

        return new BoundRangeExpression(lower, node.RangeToken, upper);
    }
}