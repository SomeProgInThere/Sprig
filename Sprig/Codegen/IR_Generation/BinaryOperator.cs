using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IR_Generation;

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

internal sealed class IRBinaryOperator(
    SyntaxKind syntaxKind, 
    BinaryOperator kind, 
    TypeSymbol leftType, 
    TypeSymbol rightType, 
    TypeSymbol type
) {

    public IRBinaryOperator(SyntaxKind syntaxKind, BinaryOperator operatorKind, TypeSymbol operandType, TypeSymbol type)
        : this(syntaxKind, operatorKind, operandType, operandType, type) {}

    public IRBinaryOperator(SyntaxKind syntaxKind, BinaryOperator operatorKind, TypeSymbol type)
        : this(syntaxKind, operatorKind, type, type, type) {}

    public static IRBinaryOperator? Bind(SyntaxKind kind, TypeSymbol leftType, TypeSymbol rightType) {
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

    private static readonly IRBinaryOperator[] operators = [
        // Interget operations
        new IRBinaryOperator(SyntaxKind.PlusToken,        BinaryOperator.Add,       TypeSymbol.Int32),
        new IRBinaryOperator(SyntaxKind.MinusToken,       BinaryOperator.Substact,  TypeSymbol.Int32),
        new IRBinaryOperator(SyntaxKind.StarToken,        BinaryOperator.Multiply,  TypeSymbol.Int32),
        new IRBinaryOperator(SyntaxKind.SlashToken,       BinaryOperator.Divide,    TypeSymbol.Int32),
        new IRBinaryOperator(SyntaxKind.DoubleSlashToken, BinaryOperator.Remainder, TypeSymbol.Int32),
        
        // Float operations
        new IRBinaryOperator(SyntaxKind.PlusToken,        BinaryOperator.Add,       TypeSymbol.Double),
        new IRBinaryOperator(SyntaxKind.MinusToken,       BinaryOperator.Substact,  TypeSymbol.Double),
        new IRBinaryOperator(SyntaxKind.StarToken,        BinaryOperator.Multiply,  TypeSymbol.Double),
        new IRBinaryOperator(SyntaxKind.SlashToken,       BinaryOperator.Divide,    TypeSymbol.Double),
        new IRBinaryOperator(SyntaxKind.DoubleSlashToken, BinaryOperator.Remainder, TypeSymbol.Double),

        // Bitwise operations
        new IRBinaryOperator(SyntaxKind.AmpersandToken,        BinaryOperator.BitwiseAnd,    TypeSymbol.Int32), 
        new IRBinaryOperator(SyntaxKind.PipeToken,             BinaryOperator.BitwiseOr,     TypeSymbol.Int32), 
        new IRBinaryOperator(SyntaxKind.CircumflexToken,       BinaryOperator.BitwiseXor,    TypeSymbol.Int32),
        new IRBinaryOperator(SyntaxKind.DoubleLeftArrowToken,  BinaryOperator.BitshiftLeft,  TypeSymbol.Int32), 
        new IRBinaryOperator(SyntaxKind.DoubleRightArrowToken, BinaryOperator.BitshiftRight, TypeSymbol.Int32), 

        // Integer Comparisions
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken,     BinaryOperator.Equals,              TypeSymbol.Int32, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,       BinaryOperator.NotEquals,           TypeSymbol.Int32, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.LeftArrowToken,        BinaryOperator.GreaterThan,         TypeSymbol.Int32, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.LeftArrowEqualsToken,  BinaryOperator.GreaterThanEqualsTo, TypeSymbol.Int32, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.RightArrowToken,       BinaryOperator.LesserThan,          TypeSymbol.Int32, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.RightArrowEqualsToken, BinaryOperator.LesserThanEqualsTo,  TypeSymbol.Int32, TypeSymbol.Boolean),

        // Float Comparisions
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken,     BinaryOperator.Equals,              TypeSymbol.Double, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,       BinaryOperator.NotEquals,           TypeSymbol.Double, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.LeftArrowToken,        BinaryOperator.GreaterThan,         TypeSymbol.Double, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.LeftArrowEqualsToken,  BinaryOperator.GreaterThanEqualsTo, TypeSymbol.Double, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.RightArrowToken,       BinaryOperator.LesserThan,          TypeSymbol.Double, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.RightArrowEqualsToken, BinaryOperator.LesserThanEqualsTo,  TypeSymbol.Double, TypeSymbol.Boolean),

        // Any Comparisions
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperator.Equals,    TypeSymbol.Any, TypeSymbol.Any),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,   BinaryOperator.NotEquals, TypeSymbol.Any, TypeSymbol.Any),

        // Logical operations
        new IRBinaryOperator(SyntaxKind.DoubleAmpersandToken, BinaryOperator.LogicalAnd, TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.DoublePipeToken,      BinaryOperator.LogicalOr,  TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken,    BinaryOperator.Equals,     TypeSymbol.Boolean),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,      BinaryOperator.NotEquals,  TypeSymbol.Boolean),

        // String operations
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperator.Equals,    TypeSymbol.String),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,   BinaryOperator.NotEquals, TypeSymbol.String),
        new IRBinaryOperator(SyntaxKind.PlusToken,         BinaryOperator.Add,       TypeSymbol.String),
    ];
}