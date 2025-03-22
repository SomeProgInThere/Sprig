using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IR;

internal enum BinaryOperator {
    Add,
    Substact,
    Multiply,
    Divide,
    Remainder,
    LogicalAnd,
    LogicalOr,
    Equals,
    NotEquals,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    BitshiftLeft,
    BitshiftRight,
    GreaterThan,
    GreaterThanEqualsTo,
    LesserThan,
    LesserThanEqualsTo,
}

internal sealed class IR_BinaryOperator(
    SyntaxKind syntaxKind, 
    BinaryOperator kind, 
    TypeSymbol leftType, 
    TypeSymbol rightType, 
    TypeSymbol type
) {

    public IR_BinaryOperator(SyntaxKind syntaxKind, BinaryOperator operatorKind, TypeSymbol operandType, TypeSymbol type)
        : this(syntaxKind, operatorKind, operandType, operandType, type) {}

    public IR_BinaryOperator(SyntaxKind syntaxKind, BinaryOperator operatorKind, TypeSymbol type)
        : this(syntaxKind, operatorKind, type, type, type) {}

    public static IR_BinaryOperator? Bind(SyntaxKind kind, TypeSymbol leftType, TypeSymbol rightType) {
        foreach (var op in operators) {
            if (op.SyntaxKind == kind && op.LeftType == leftType && op.RightType == rightType) {
                return op;
            }
        }

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public BinaryOperator Kind { get; } = kind;
    public TypeSymbol LeftType { get; } = leftType;
    public TypeSymbol RightType { get; } = rightType;
    public TypeSymbol Type { get; } = type;

    private static readonly IR_BinaryOperator[] operators = [
        // Interget operations
        new IR_BinaryOperator(SyntaxKind.PlusToken,        BinaryOperator.Add,       TypeSymbol.Int32),
        new IR_BinaryOperator(SyntaxKind.MinusToken,       BinaryOperator.Substact,  TypeSymbol.Int32),
        new IR_BinaryOperator(SyntaxKind.StarToken,        BinaryOperator.Multiply,  TypeSymbol.Int32),
        new IR_BinaryOperator(SyntaxKind.SlashToken,       BinaryOperator.Divide,    TypeSymbol.Int32),
        new IR_BinaryOperator(SyntaxKind.PercentToken,     BinaryOperator.Remainder, TypeSymbol.Int32),
        
        // Float operations
        new IR_BinaryOperator(SyntaxKind.PlusToken,        BinaryOperator.Add,       TypeSymbol.Double),
        new IR_BinaryOperator(SyntaxKind.MinusToken,       BinaryOperator.Substact,  TypeSymbol.Double),
        new IR_BinaryOperator(SyntaxKind.StarToken,        BinaryOperator.Multiply,  TypeSymbol.Double),
        new IR_BinaryOperator(SyntaxKind.SlashToken,       BinaryOperator.Divide,    TypeSymbol.Double),
        new IR_BinaryOperator(SyntaxKind.PercentToken,     BinaryOperator.Remainder, TypeSymbol.Double),

        // Bitwise operations
        new IR_BinaryOperator(SyntaxKind.AmpersandToken,        BinaryOperator.BitwiseAnd,    TypeSymbol.Int32), 
        new IR_BinaryOperator(SyntaxKind.PipeToken,             BinaryOperator.BitwiseOr,     TypeSymbol.Int32), 
        new IR_BinaryOperator(SyntaxKind.CircumflexToken,       BinaryOperator.BitwiseXor,    TypeSymbol.Int32),
        new IR_BinaryOperator(SyntaxKind.DoubleLeftArrowToken,  BinaryOperator.BitshiftLeft,  TypeSymbol.Int32), 
        new IR_BinaryOperator(SyntaxKind.DoubleRightArrowToken, BinaryOperator.BitshiftRight, TypeSymbol.Int32), 

        // Integer Comparisions
        new IR_BinaryOperator(SyntaxKind.DoubleEqualsToken,     BinaryOperator.Equals,              TypeSymbol.Int32, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.BangEqualsToken,       BinaryOperator.NotEquals,           TypeSymbol.Int32, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.LeftArrowToken,        BinaryOperator.GreaterThan,         TypeSymbol.Int32, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.LeftArrowEqualsToken,  BinaryOperator.GreaterThanEqualsTo, TypeSymbol.Int32, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.RightArrowToken,       BinaryOperator.LesserThan,          TypeSymbol.Int32, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.RightArrowEqualsToken, BinaryOperator.LesserThanEqualsTo,  TypeSymbol.Int32, TypeSymbol.Boolean),

        // Float Comparisions
        new IR_BinaryOperator(SyntaxKind.DoubleEqualsToken,     BinaryOperator.Equals,              TypeSymbol.Double, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.BangEqualsToken,       BinaryOperator.NotEquals,           TypeSymbol.Double, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.LeftArrowToken,        BinaryOperator.GreaterThan,         TypeSymbol.Double, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.LeftArrowEqualsToken,  BinaryOperator.GreaterThanEqualsTo, TypeSymbol.Double, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.RightArrowToken,       BinaryOperator.LesserThan,          TypeSymbol.Double, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.RightArrowEqualsToken, BinaryOperator.LesserThanEqualsTo,  TypeSymbol.Double, TypeSymbol.Boolean),

        // Any Comparisions
        new IR_BinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperator.Equals,    TypeSymbol.Any, TypeSymbol.Any),
        new IR_BinaryOperator(SyntaxKind.BangEqualsToken,   BinaryOperator.NotEquals, TypeSymbol.Any, TypeSymbol.Any),

        // Logical operations
        new IR_BinaryOperator(SyntaxKind.DoubleAmpersandToken, BinaryOperator.LogicalAnd, TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.DoublePipeToken,      BinaryOperator.LogicalOr,  TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.DoubleEqualsToken,    BinaryOperator.Equals,     TypeSymbol.Boolean),
        new IR_BinaryOperator(SyntaxKind.BangEqualsToken,      BinaryOperator.NotEquals,  TypeSymbol.Boolean),

        // String operations
        new IR_BinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperator.Equals,    TypeSymbol.String),
        new IR_BinaryOperator(SyntaxKind.BangEqualsToken,   BinaryOperator.NotEquals, TypeSymbol.String),
        new IR_BinaryOperator(SyntaxKind.PlusToken,         BinaryOperator.Add,       TypeSymbol.String),
    ];
}