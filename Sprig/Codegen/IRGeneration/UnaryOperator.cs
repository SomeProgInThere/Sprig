using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IRGeneration;

internal enum UnaryOperator {
    Identity,
    Negetion,
    BitwiseNot,
    LogicalNot,
}

internal sealed class IRUnaryOperator(SyntaxKind syntaxKind, UnaryOperator kind, TypeSymbol operandType, TypeSymbol type) {

    public IRUnaryOperator(SyntaxKind syntaxKind, UnaryOperator operatorKind, TypeSymbol operandType)
        : this(syntaxKind, operatorKind, operandType, operandType) {}

    public static IRUnaryOperator? Bind(SyntaxKind kind, TypeSymbol operandType) {
        foreach (var op in operators)
            if (op.SyntaxKind == kind && op.OperandType == operandType)
                return op;

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public UnaryOperator Kind { get; } = kind;
    public TypeSymbol OperandType { get; } = operandType;
    public TypeSymbol Type { get; } = type;

    private static readonly IRUnaryOperator[] operators = [
        new IRUnaryOperator(SyntaxKind.PlusToken,  UnaryOperator.Identity,   TypeSymbol.Int),
        new IRUnaryOperator(SyntaxKind.MinusToken, UnaryOperator.Negetion,   TypeSymbol.Int),
        new IRUnaryOperator(SyntaxKind.TildeToken, UnaryOperator.BitwiseNot, TypeSymbol.Int),
        
        new IRUnaryOperator(SyntaxKind.BangToken,  UnaryOperator.LogicalNot, TypeSymbol.Bool),
    ];
}