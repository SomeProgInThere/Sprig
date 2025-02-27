using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal abstract class BoundExpression : BoundNode {
    public abstract TypeSymbol Type { get; }
}

internal sealed class BoundLiteralExpression 
    : BoundExpression {

    public BoundLiteralExpression(object value) {
        if (value is bool)
            Type = TypeSymbol.Boolean;
        else if (value is int)
            Type = TypeSymbol.Int;
        else if (value is string)
            Type = TypeSymbol.String;
        else
            throw new Exception($"Unexpected literal '{value}' of type '{value.GetType()}'");

        Value = value;
    }

    public object Value { get; }

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override TypeSymbol Type { get; }
}

internal sealed class BoundUnaryExpression(BoundExpression operand, UnaryOperator op)
    : BoundExpression {

    public UnaryOperator Operator { get; } = op;
    public BoundExpression Operand { get; } = operand;

    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operand.Type;
}

internal sealed class BoundVariableExpression(VariableSymbol? variable)
    : BoundExpression {

    public VariableSymbol? Variable { get; } = variable;
    
    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    public override TypeSymbol Type => Variable?.Type ?? TypeSymbol.Int;
}

internal sealed class BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
    : BoundExpression {

    public VariableSymbol Variable { get; } = variable;
    public BoundExpression Expression { get; } = expression;

    public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    public override TypeSymbol Type => Expression.Type;
}

internal sealed class BoundBinaryExpression(BoundExpression left, BoundExpression right, BinaryOperator op) 
    : BoundExpression {

    public BinaryOperator Operator { get; } = op;
    public BoundExpression Left { get; } = left;
    public BoundExpression Right { get; } = right;

    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override TypeSymbol Type => Operator.ResultType;
}

internal sealed class BoundRangeExpression(BoundExpression lower, SyntaxToken rangeToken, BoundExpression upper)
    : BoundExpression
{
    public BoundExpression Lower { get; } = lower;
    public SyntaxToken RangeToken { get; } = rangeToken;
    public BoundExpression Upper { get; } = upper;

    public override BoundNodeKind Kind => BoundNodeKind.RangeExpression;
    public override TypeSymbol Type => Lower.Type;
}