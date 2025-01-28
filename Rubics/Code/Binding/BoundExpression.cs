
namespace Rubics.Code.Binding;

internal abstract class BoundExpression : BoundNode {
    public abstract Type Type { get; }
}

internal sealed class BoundLiteralExpression(object value) 
    : BoundExpression {

    public object Value { get; } = value;

    public override BoundKind Kind => BoundKind.LiteralExpression;
    public override Type Type => Value.GetType();
}

internal sealed class BoundUnaryExpression(BoundExpression operand, UnaryOperator op)
    : BoundExpression {

    public UnaryOperator Op { get; } = op;
    public BoundExpression Operand { get; } = operand;

    public override BoundKind Kind => BoundKind.UnaryExpression;
    public override Type Type => Operand.Type;
}

internal sealed class BoundVariableExpression(string name, Type type)
    : BoundExpression {

    public string Name { get; } = name;

    public override BoundKind Kind => BoundKind.VariableExpression;
    public override Type Type => type;
}

internal sealed class BoundAssignmentExpression(string name, BoundExpression expression)
    : BoundExpression {

    public string Name { get; } = name;
    public BoundExpression Expression { get; } = expression;

    public override BoundKind Kind => BoundKind.AssignmentExpression;
    public override Type Type => Expression.Type;
}

internal sealed class BoundBinaryExpression(BoundExpression left, BoundExpression right, BinaryOperator op) 
    : BoundExpression {

    public BinaryOperator Op { get; } = op;
    public BoundExpression Left { get; } = left;
    public BoundExpression Right { get; } = right;

    public override BoundKind Kind => BoundKind.BinaryExpression;
    public override Type Type => Op.ResultType;
}