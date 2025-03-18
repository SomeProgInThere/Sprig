using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IR_Generation;

internal enum UnaryOperator {
    Identity,
    Negetion,
    BitwiseNot,
    LogicalNot,
}

internal sealed class IR_UnaryOperator(SyntaxKind syntaxKind, UnaryOperator kind, TypeSymbol operandType, TypeSymbol type) {

    public IR_UnaryOperator(SyntaxKind syntaxKind, UnaryOperator operatorKind, TypeSymbol operandType)
        : this(syntaxKind, operatorKind, operandType, operandType) {}

    public static IR_UnaryOperator? Bind(SyntaxKind kind, TypeSymbol operandType) {
        foreach (var op in operators)
            if (op.SyntaxKind == kind && op.OperandType == operandType)
                return op;

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public UnaryOperator Kind { get; } = kind;
    public TypeSymbol OperandType { get; } = operandType;
    public TypeSymbol Type { get; } = type;

    private static readonly IR_UnaryOperator[] operators = [
        new IR_UnaryOperator(SyntaxKind.PlusToken,  UnaryOperator.Identity,   TypeSymbol.Int32),
        new IR_UnaryOperator(SyntaxKind.MinusToken, UnaryOperator.Negetion,   TypeSymbol.Int32),
        new IR_UnaryOperator(SyntaxKind.TildeToken, UnaryOperator.BitwiseNot, TypeSymbol.Int32),
        
        new IR_UnaryOperator(SyntaxKind.BangToken,  UnaryOperator.LogicalNot, TypeSymbol.Boolean),
    ];
}