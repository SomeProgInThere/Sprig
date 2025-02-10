
using System.Collections.Immutable;
using Rubics.Code.Binding;

namespace Rubics.Code;

internal sealed class Evaluator(BoundStatement? root, Dictionary<VariableSymbol, object> variables) {
    
    public object? Evaluate() {
        EvaluateStatement(root);
        return lastValue;
    }

    private void EvaluateStatement(BoundStatement? statement) {
        switch (statement?.Kind) {
            case BoundKind.BlockStatement:
                EvaluateBlockStatement((BoundBlockStatement)statement);
                break;
            
            case BoundKind.ExpressionStatement:
                EvaluateExpressionStatement((BoundExpressionStatement)statement);
                break;

            default: 
                throw new Exception($"Undefined statement: {statement?.Kind}");
        }
    }

    private void EvaluateBlockStatement(BoundBlockStatement statement) {
        foreach (var boundStatement in statement.Statements)
            EvaluateStatement(boundStatement);
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement statement) => lastValue = EvaluateExpression(statement.Expression);
    
    private object EvaluateExpression(BoundExpression? node){
        return node?.Kind switch {
            BoundKind.LiteralExpression     => EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundKind.VariableExpression    => EvaluateVariableExpression((BoundVariableExpression)node),
            BoundKind.AssignmentExpression  => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundKind.UnaryExpression       => EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundKind.BinaryExpression      => EvaluateBinaryExpression((BoundBinaryExpression)node),
            
            _ => throw new Exception($"Undefined node: {node?.Kind}"),
        };
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression literal) => literal.Value;
    
    private object EvaluateVariableExpression(BoundVariableExpression variable) {
        if (variable.Variable is null)
            throw new Exception($"Variable: {nameof(variable.Variable)} is not initialized");
        
        return variables[variable.Variable];
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression assignment) {
        var value = EvaluateExpression(assignment.Expression);
        variables[assignment.Variable] = value;
        return value;
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression unary) {
        var operand = EvaluateExpression(unary.Operand);

        return unary.Operator.Kind switch {
            UnaryOperatorKind.Identity => (int)operand,
            UnaryOperatorKind.Negetion => -(int)operand,
            UnaryOperatorKind.BitwiseNot => ~(int)operand,
            UnaryOperatorKind.LogicalNot => !(bool)operand,

            _ => throw new Exception($"Unexpected Unary operator: {unary.Operator}"),
        };
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression binary) {
        var left = EvaluateExpression(binary.Left);
        var right = EvaluateExpression(binary.Right);

        return binary.Operator.Kind switch {
            BinaryOperatorKind.Add      => (int)left + (int)right,
            BinaryOperatorKind.Substact => (int)left - (int)right,
            BinaryOperatorKind.Multiply => (int)left * (int)right,
            BinaryOperatorKind.Divide   => (int)left / (int)right,
            BinaryOperatorKind.Modulus  => (int)left % (int)right,

            BinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BinaryOperatorKind.LogicalOr  => (bool)left || (bool)right,

            BinaryOperatorKind.BitwiseAnd => (int)left & (int)right,
            BinaryOperatorKind.BitwiseOr  => (int)left | (int)right,
            BinaryOperatorKind.BitwiseXor => (int)left ^ (int)right,

            BinaryOperatorKind.GreaterThan          => (int)left >  (int)right,
            BinaryOperatorKind.GreaterThanEqualsTo  => (int)left >= (int)right,
            BinaryOperatorKind.LesserThan           => (int)left <  (int)right,
            BinaryOperatorKind.LesserThanEqualsTo   => (int)left <= (int)right,

            BinaryOperatorKind.Equals    => Equals(left, right),
            BinaryOperatorKind.NotEquals => !Equals(left, right),

            _ => throw new Exception($"Unexpected Binary operator: {binary.Operator}"),
        };
    }

    private object? lastValue;
}

public sealed class EvaluationResult(ImmutableArray<DiagnosticMessage> diagnostics, object? result = null) {
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public object? Result { get; } = result;
}

public sealed class VariableSymbol(string name, Type type) {
    public string Name { get; } = name;
    public Type Type { get; } = type;
}