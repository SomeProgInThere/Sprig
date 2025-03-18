using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IR_Generation;

internal abstract class IR_Expression : IR_Node {
    public abstract TypeSymbol Type { get; }
}

internal sealed class IR_LiteralExpression(object value)
        : IR_Expression {

    public object Value { get; } = value;

    public override IR_NodeKind Kind => IR_NodeKind.LiteralExpression;
    public override TypeSymbol Type { get; } = value switch {
        bool    => TypeSymbol.Boolean,
        int     => TypeSymbol.Int32,
        float   => TypeSymbol.Double,
        string  => TypeSymbol.String,
        
        _ => throw new Exception($"Unexpected literal '{value}' of type '{value.GetType()}'"),
    };
}

internal sealed class IR_UnaryExpression(IR_Expression operand, IR_UnaryOperator op)
    : IR_Expression {

    public IR_UnaryOperator Operator { get; } = op;
    public IR_Expression Operand { get; } = operand;

    public override IR_NodeKind Kind => IR_NodeKind.UnaryExpression;
    public override TypeSymbol Type => Operand.Type;
}

internal sealed class IR_VariableExpression(VariableSymbol variable)
    : IR_Expression {

    public VariableSymbol Variable { get; } = variable;
    
    public override IR_NodeKind Kind => IR_NodeKind.VariableExpression;
    public override TypeSymbol Type => Variable?.Type ?? TypeSymbol.Int32;
}

internal sealed class IR_AssignmentExpression(VariableSymbol variable, IR_Expression expression)
    : IR_Expression {

    public VariableSymbol Variable { get; } = variable;
    public IR_Expression Expression { get; } = expression;

    public override IR_NodeKind Kind => IR_NodeKind.AssignmentExpression;
    public override TypeSymbol Type => Expression.Type;
}

internal sealed class IR_BinaryExpression(IR_Expression left, IR_Expression right, IRBinaryOperator op) 
    : IR_Expression {

    public IRBinaryOperator Operator { get; } = op;
    public IR_Expression Left { get; } = left;
    public IR_Expression Right { get; } = right;

    public override IR_NodeKind Kind => IR_NodeKind.BinaryExpression;
    public override TypeSymbol Type => Operator.Type;
}

internal sealed class IR_RangeExpression(IR_Expression lower, IR_Expression upper)
    : IR_Expression
{
    public IR_Expression Lower { get; } = lower;
    public IR_Expression Upper { get; } = upper;

    public override IR_NodeKind Kind => IR_NodeKind.RangeExpression;
    public override TypeSymbol Type => Lower.Type;
}

internal sealed class IR_CallExpression(FunctionSymbol function, ImmutableArray<IR_Expression> arguments)
    : IR_Expression {

    public FunctionSymbol Function { get; } = function;
    public ImmutableArray<IR_Expression> Arguments { get; } = arguments;

    public override IR_NodeKind Kind => IR_NodeKind.CallExpression;
    public override TypeSymbol Type => Function.Type;
}

internal class IR_CastExpression(TypeSymbol type, IR_Expression expression)
        : IR_Expression {
        
    public IR_Expression Expression { get; } = expression;

    public override IR_NodeKind Kind => IR_NodeKind.CastExpression;
    public override TypeSymbol Type => type;
}

internal sealed class IR_ErrorExpression
    : IR_Expression {

    public override IR_NodeKind Kind => IR_NodeKind.ErrorExpression;
    public override TypeSymbol Type => TypeSymbol.Error;
}