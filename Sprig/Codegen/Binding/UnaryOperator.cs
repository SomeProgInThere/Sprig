using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Binding;

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

internal sealed class BoundUnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType) {

    public BoundUnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind operatorKind, TypeSymbol operandType)
        : this(syntaxKind, operatorKind, operandType, operandType) {}

    public static BoundUnaryOperator? Bind(SyntaxKind kind, TypeSymbol operandType) {
        foreach (var op in operators)
            if (op.SyntaxKind == kind && op.OperandType == operandType)
                return op;

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public UnaryOperatorKind Kind { get; } = kind;
    public TypeSymbol OperandType { get; } = operandType;
    public TypeSymbol ResultType { get; } = resultType;

    private static readonly BoundUnaryOperator[] operators = [
        new BoundUnaryOperator(SyntaxKind.PlusToken,  UnaryOperatorKind.Identity,   TypeSymbol.Int),
        new BoundUnaryOperator(SyntaxKind.MinusToken, UnaryOperatorKind.Negetion,   TypeSymbol.Int),
        new BoundUnaryOperator(SyntaxKind.TildeToken, UnaryOperatorKind.BitwiseNot, TypeSymbol.Int),

        new BoundUnaryOperator(SyntaxKind.DoublePlusToken,  UnaryOperatorKind.PostIncrement, TypeSymbol.Int),
        new BoundUnaryOperator(SyntaxKind.DoubleMinusToken, UnaryOperatorKind.PostDecrement, TypeSymbol.Int),
        new BoundUnaryOperator(SyntaxKind.DoublePlusToken,  UnaryOperatorKind.PreIncrement,  TypeSymbol.Int),
        new BoundUnaryOperator(SyntaxKind.DoubleMinusToken, UnaryOperatorKind.PreDecrement,  TypeSymbol.Int),
        
        new BoundUnaryOperator(SyntaxKind.BangToken,  UnaryOperatorKind.LogicalNot, TypeSymbol.Bool),
    ];
}