using Sprig.Code.Symbols;
using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal enum UnaryOperatorKind {
    Identity,
    Negetion,
    BitwiseNot,
    LogicalNot,
    PreIncrement,
    PreDecrement,
    PostIncrement,
    PostDecrement,
}

internal sealed class UnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType) {

    public UnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind operatorKind, TypeSymbol operandType)
        : this(syntaxKind, operatorKind, operandType, operandType) {}

    public static UnaryOperator? Bind(SyntaxKind kind, TypeSymbol operandType) {
        foreach (var op in operators)
            if (op.SyntaxKind == kind && op.OperandType == operandType)
                return op;

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public UnaryOperatorKind Kind { get; } = kind;
    public TypeSymbol OperandType { get; } = operandType;
    public TypeSymbol ResultType { get; } = resultType;

    private static readonly UnaryOperator[] operators = [
        new UnaryOperator(SyntaxKind.PlusToken,  UnaryOperatorKind.Identity,   TypeSymbol.Int),
        new UnaryOperator(SyntaxKind.MinusToken, UnaryOperatorKind.Negetion,   TypeSymbol.Int),
        new UnaryOperator(SyntaxKind.TildeToken, UnaryOperatorKind.BitwiseNot, TypeSymbol.Int),

        new UnaryOperator(SyntaxKind.DoublePlusToken,  UnaryOperatorKind.PostIncrement, TypeSymbol.Int),
        new UnaryOperator(SyntaxKind.DoubleMinusToken, UnaryOperatorKind.PostDecrement, TypeSymbol.Int),
        new UnaryOperator(SyntaxKind.DoublePlusToken,  UnaryOperatorKind.PreIncrement,  TypeSymbol.Int),
        new UnaryOperator(SyntaxKind.DoubleMinusToken, UnaryOperatorKind.PreDecrement,  TypeSymbol.Int),
        
        new UnaryOperator(SyntaxKind.BangToken,  UnaryOperatorKind.LogicalNot, TypeSymbol.Boolean),
    ];
}