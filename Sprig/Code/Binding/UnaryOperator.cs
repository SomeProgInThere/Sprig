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

internal sealed class UnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind kind, Type operandType, Type resultType) {

    public UnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind operatorKind, Type operandType)
        : this(syntaxKind, operatorKind, operandType, operandType) {}

    public static UnaryOperator? Bind(SyntaxKind kind, Type operandType) {
        foreach (var op in operators)
            if (op.SyntaxKind == kind && op.OperandType == operandType)
                return op;

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public UnaryOperatorKind Kind { get; } = kind;
    public Type OperandType { get; } = operandType;
    public Type ResultType { get; } = resultType;

    private static readonly UnaryOperator[] operators = [
        new UnaryOperator(SyntaxKind.PlusToken,  UnaryOperatorKind.Identity,   typeof(int)),
        new UnaryOperator(SyntaxKind.MinusToken, UnaryOperatorKind.Negetion,   typeof(int)),
        new UnaryOperator(SyntaxKind.TildeToken, UnaryOperatorKind.BitwiseNot, typeof(int)),

        new UnaryOperator(SyntaxKind.DoublePlusToken,  UnaryOperatorKind.PostIncrement, typeof(int)),
        new UnaryOperator(SyntaxKind.DoubleMinusToken, UnaryOperatorKind.PostDecrement, typeof(int)),
        new UnaryOperator(SyntaxKind.DoublePlusToken,  UnaryOperatorKind.PreIncrement,  typeof(int)),
        new UnaryOperator(SyntaxKind.DoubleMinusToken, UnaryOperatorKind.PreDecrement,  typeof(int)),
        
        new UnaryOperator(SyntaxKind.BangToken,  UnaryOperatorKind.LogicalNot, typeof(bool)),
    ];
}