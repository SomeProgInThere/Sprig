using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IRGeneration;

internal abstract class IRExpression : IRNode {
    public abstract TypeSymbol Type { get; }
}

internal sealed class IRLiteralExpression(object value)
        : IRExpression {

    public object Value { get; } = value;

    public override IRNodeKind Kind => IRNodeKind.LiteralExpression;
    public override TypeSymbol Type { get; } = value switch {
        bool    => TypeSymbol.Bool,
        int     => TypeSymbol.Int,
        float   => TypeSymbol.Float,
        string  => TypeSymbol.String,
        
        _ => throw new Exception($"Unexpected literal '{value}' of type '{value.GetType()}'"),
    };
}

internal sealed class IRUnaryExpression(IRExpression operand, IRUnaryOperator op)
    : IRExpression {

    public IRUnaryOperator Operator { get; } = op;
    public IRExpression Operand { get; } = operand;

    public override IRNodeKind Kind => IRNodeKind.UnaryExpression;
    public override TypeSymbol Type => Operand.Type;
}

internal sealed class IRVariableExpression(VariableSymbol variable)
    : IRExpression {

    public VariableSymbol Variable { get; } = variable;
    
    public override IRNodeKind Kind => IRNodeKind.VariableExpression;
    public override TypeSymbol Type => Variable?.Type ?? TypeSymbol.Int;
}

internal sealed class IRAssignmentExpression(VariableSymbol variable, IRExpression expression)
    : IRExpression {

    public VariableSymbol Variable { get; } = variable;
    public IRExpression Expression { get; } = expression;

    public override IRNodeKind Kind => IRNodeKind.AssignmentExpression;
    public override TypeSymbol Type => Expression.Type;
}

internal sealed class IRBinaryExpression(IRExpression left, IRExpression right, IRBinaryOperator op) 
    : IRExpression {

    public IRBinaryOperator Operator { get; } = op;
    public IRExpression Left { get; } = left;
    public IRExpression Right { get; } = right;

    public override IRNodeKind Kind => IRNodeKind.BinaryExpression;
    public override TypeSymbol Type => Operator.Type;
}

internal sealed class IRRangeExpression(IRExpression lower, IRExpression upper)
    : IRExpression
{
    public IRExpression Lower { get; } = lower;
    public IRExpression Upper { get; } = upper;

    public override IRNodeKind Kind => IRNodeKind.RangeExpression;
    public override TypeSymbol Type => Lower.Type;
}

internal sealed class IRCallExpression(FunctionSymbol function, ImmutableArray<IRExpression> arguments)
    : IRExpression {

    public FunctionSymbol Function { get; } = function;
    public ImmutableArray<IRExpression> Arguments { get; } = arguments;

    public override IRNodeKind Kind => IRNodeKind.CallExpression;
    public override TypeSymbol Type => Function.Type;
}

internal class IRCastExpression(TypeSymbol type, IRExpression expression)
        : IRExpression {
        
    public IRExpression Expression { get; } = expression;

    public override IRNodeKind Kind => IRNodeKind.CastExpression;
    public override TypeSymbol Type => type;
}

internal sealed class IRErrorExpression
    : IRExpression {

    public override IRNodeKind Kind => IRNodeKind.ErrorExpression;
    public override TypeSymbol Type => TypeSymbol.Error;
}