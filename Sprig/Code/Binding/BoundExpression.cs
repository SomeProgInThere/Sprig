using System.Collections.Immutable;
using Sprig.Code.Symbols;
using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal abstract class BoundExpression : BoundNode {
    public abstract TypeSymbol Type { get; }
}

internal sealed class BoundLiteralExpression(object value)
        : BoundExpression {

    public object Value { get; } = value;

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override TypeSymbol Type { get; } = value switch {
        bool => TypeSymbol.Boolean,
        int => TypeSymbol.Int,
        string => TypeSymbol.String,
        _ => throw new Exception($"Unexpected literal '{value}' of type '{value.GetType()}'"),
    };
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

internal sealed class BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
    : BoundExpression {

    public FunctionSymbol Function { get; } = function;
    public ImmutableArray<BoundExpression> Arguments { get; } = arguments;

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
    public override TypeSymbol Type => Function.Type;
}

internal sealed class BoundErrorExpression
    : BoundExpression {

    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    public override TypeSymbol Type => TypeSymbol.Error;
}