
using Rubics.Code.Binding;

namespace Rubics.Code;

internal sealed class Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables) {
    
    public object Evaluate() {
        return EvaluateExpression(root);
    }

    private object EvaluateExpression(BoundExpression node) {
        if (node is BoundLiteralExpression literal)
            return literal.Value;

        if (node is BoundVariableExpression variable)
            return variables[variable.Variable];
        
        if (node is BoundAssignmentExpression assignment) {
            var value = EvaluateExpression(assignment.Expression);
            variables[assignment.Variable] = value;
            return value;
        }

        if (node is BoundUnaryExpression unary) {
            var operand = EvaluateExpression(unary.Operand);

            return unary.Op.Kind switch {
                UnaryOperatorKind.Identity => (int)operand,
                UnaryOperatorKind.Negetion => -(int)operand,
                UnaryOperatorKind.LogicalNot => !(bool)operand,
                
                _ => throw new Exception($"Unexpected Unary operator: {unary.Op}"),
            };
        }

        if (node is BoundBinaryExpression binary) {
            var left = EvaluateExpression(binary.Left);
            var right = EvaluateExpression(binary.Right);

            return binary.Op.Kind switch {
                BinaryOperatorKind.Add      => (int)left + (int)right,
                BinaryOperatorKind.Substact => (int)left - (int)right,
                BinaryOperatorKind.Multiply => (int)left * (int)right,
                BinaryOperatorKind.Divide   => (int)left / (int)right,
                BinaryOperatorKind.Modulus  => (int)left % (int)right,
                
                BinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BinaryOperatorKind.LogicalOr  => (bool)left || (bool)right,

                BinaryOperatorKind.Equals    => Equals(left, right),
                BinaryOperatorKind.NotEquals => !Equals(left, right),

                _ => throw new Exception($"Unexpected Binary operator: {binary.Op}"),
            };
        }

        throw new Exception($"Unexpected node: {node.Kind}");
    }
}