using System.Collections.Immutable;

using Sprig.Code.Binding;
using Sprig.Code.Syntax;

namespace Sprig.Code;

internal sealed class Evaluator(BoundBlockStatement? root, Dictionary<VariableSymbol, object> variables) {
    
    public object? Evaluate() {
        var labelMap = new Dictionary<LabelSymbol, int>();

        for (var i = 0; i < root?.Statements.Length; i++) {
            if (root.Statements[i] is BoundLableStatement statement)
                labelMap.Add(statement.Label, i + 1);
        }

        var index = 0;
        while (index < root.Statements.Length) {
            var statement = root.Statements[index];
            switch (statement.Kind) {
                case BoundNodeKind.VariableDeclarationStatement:
                    EvaluateVariableDeclaration((BoundVariableDeclarationStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.AssignOperationStatement:
                    EvaluateAssignOperationStatement((BoundAssignOperationStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.GotoStatement:
                    var gotoStatement = (BoundGotoStatement)statement;
                    index = labelMap[gotoStatement.Label];
                    break;

                case BoundNodeKind.ConditionalGotoStatement:
                    var conditionalGotoStatement = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);
                    
                    if (condition && conditionalGotoStatement.Jump || !condition && !conditionalGotoStatement.Jump)
                        index = labelMap[conditionalGotoStatement.Label];
                    else 
                        index++;
                    break;
                
                case BoundNodeKind.LabelStatement:
                    index++;
                    break;

                default:
                    throw new Exception($"Undefined statement: {statement.Kind}");
            }
        } 

        return lastValue;
    }

    private void EvaluateAssignOperationStatement(BoundAssignOperationStatement node) {
        var value = (int)EvaluateExpression(node.Expression);

        if (node.Variable == null) 
            return;
        
        var kind = node.AssignOperatorToken.Kind;
        variables[node.Variable] = kind switch {
            SyntaxKind.PlusEqualsToken          => (int)variables[node.Variable] + value,
            SyntaxKind.MinusEqualsToken         => (int)variables[node.Variable] - value,
            SyntaxKind.StarEqualsToken          => (int)variables[node.Variable] * value,
            SyntaxKind.SlashEqualsToken         => (int)variables[node.Variable] / value,
            SyntaxKind.PercentEqualsToken       => (int)variables[node.Variable] % value,
            SyntaxKind.AmpersandEqualsToken     => (int)variables[node.Variable] & value,
            SyntaxKind.PipeEqualsToken          => (int)variables[node.Variable] | value,
            SyntaxKind.CircumflexEqualsToken    => (int)variables[node.Variable] ^ value,
                
            _ => throw new Exception($"Undefined assignment operator: {kind}"),
        };

        lastValue = variables[node.Variable];
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclarationStatement node) {
        var value = EvaluateExpression(node.Initializer);
        variables[node.Variable] = value;
        lastValue = value;
    }
    
    private void EvaluateExpressionStatement(BoundExpressionStatement node) => lastValue = EvaluateExpression(node.Expression);
    
    private object EvaluateExpression(BoundExpression? node) {
        return node?.Kind switch {
            BoundNodeKind.LiteralExpression     => EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.VariableExpression    => EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.AssignmentExpression  => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.UnaryExpression       => EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression      => EvaluateBinaryExpression((BoundBinaryExpression)node),
            BoundNodeKind.RangeExpression       => EvaluateRangeExpression((BoundRangeExpression)node),
            
            _ => throw new Exception($"Undefined node: {node?.Kind}"),
        };
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression literal) => literal.Value;
    
    private object EvaluateVariableExpression(BoundVariableExpression node) {
        if (node.Variable is null)
            throw new Exception($"Variable: {nameof(node.Variable)} is not initialized");
        
        return variables[node.Variable];
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression node) {
        var value = EvaluateExpression(node.Expression);
        variables[node.Variable] = value;
        return value;
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression node) {
        var operand = EvaluateExpression(node.Operand);

        switch (node.Operator.Kind) {
            case UnaryOperatorKind.Identity:
                return (int)operand;
                
            case UnaryOperatorKind.Negetion:
                return -(int)operand;

            case UnaryOperatorKind.BitwiseNot:
                return ~(int)operand;

            case UnaryOperatorKind.LogicalNot:
                return !(bool)operand;

            case UnaryOperatorKind.PostIncrement: {
                var value = (int)operand;
                value++;
                return value;
            }

            case UnaryOperatorKind.PostDecrement: {
                var value = (int)operand;
                value--;
                return value;
            }
                
            case UnaryOperatorKind.PreIncrement: {
                var value = (int)operand;
                ++value;
                return value;
            }
            
            case UnaryOperatorKind.PreDecrement: {
                var value = (int)operand;
                --value;
                return value;
            }
            
            default:
                throw new Exception($"Unexpected Unary operator: {node.Operator}");
        }
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression node) {
        var left = EvaluateExpression(node.Left);
        var right = EvaluateExpression(node.Right);

        return node.Operator.Kind switch {
            BinaryOperatorKind.Add      => (int)left + (int)right,
            BinaryOperatorKind.Substact => (int)left - (int)right,
            BinaryOperatorKind.Multiply => (int)left * (int)right,
            BinaryOperatorKind.Divide   => (int)left / (int)right,
            BinaryOperatorKind.Modulus  => (int)left % (int)right,

            BinaryOperatorKind.Remainder    => Math.DivRem((int)left, (int)right).Remainder,
            BinaryOperatorKind.RaisePower   => (int)Math.Pow((int)left, (int)right),

            BinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BinaryOperatorKind.LogicalOr  => (bool)left || (bool)right,

            BinaryOperatorKind.BitwiseAnd => (int)left & (int)right,
            BinaryOperatorKind.BitwiseOr  => (int)left | (int)right,
            BinaryOperatorKind.BitwiseXor => (int)left ^ (int)right,

            BinaryOperatorKind.BitshiftLeft  => (int)left >> (int)right,
            BinaryOperatorKind.BitshiftRight => (int)left << (int)right,

            BinaryOperatorKind.GreaterThan          => (int)left >  (int)right,
            BinaryOperatorKind.GreaterThanEqualsTo  => (int)left >= (int)right,
            BinaryOperatorKind.LesserThan           => (int)left <  (int)right,
            BinaryOperatorKind.LesserThanEqualsTo   => (int)left <= (int)right,

            BinaryOperatorKind.Equals    => Equals(left, right),
            BinaryOperatorKind.NotEquals => !Equals(left, right),

            _ => throw new Exception($"Unexpected Binary operator: {node.Operator}"),
        };
    }

    private object EvaluateRangeExpression(BoundRangeExpression node) {
        var lower = EvaluateExpression(node.Lower);
        var upper = EvaluateExpression(node.Upper);
        return (lower, upper);
    }

    private object? lastValue;
}

public sealed class EvaluationResult(ImmutableArray<DiagnosticMessage> diagnostics, object? result = null) {
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public object? Result { get; } = result;
}