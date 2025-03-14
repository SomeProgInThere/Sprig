using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IRGeneration;

internal enum BinaryOperator {
    Add,
    AddAssign,
    Substact,
    Multiply,
    Divide,
    Remainder,
    RaisePower,
    Modulus,
    LogicalAnd,
    LogicalOr,
    LogicalXor,
    Equals,
    NotEquals,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    BitwiseNot,
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
        new IRBinaryOperator(SyntaxKind.PlusToken,                BinaryOperator.Add,         TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.MinusToken,               BinaryOperator.Substact,    TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.StarToken,                BinaryOperator.Multiply,    TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.SlashToken,               BinaryOperator.Divide,      TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.PercentToken,             BinaryOperator.Modulus,     TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.DoubleSlashToken,         BinaryOperator.Remainder,   TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.DoubleStarToken,          BinaryOperator.RaisePower,  TypeSymbol.Int),
        
        // Float operations
        new IRBinaryOperator(SyntaxKind.PlusToken,                BinaryOperator.Add,         TypeSymbol.Float),
        new IRBinaryOperator(SyntaxKind.MinusToken,               BinaryOperator.Substact,    TypeSymbol.Float),
        new IRBinaryOperator(SyntaxKind.StarToken,                BinaryOperator.Multiply,    TypeSymbol.Float),
        new IRBinaryOperator(SyntaxKind.SlashToken,               BinaryOperator.Divide,      TypeSymbol.Float),
        new IRBinaryOperator(SyntaxKind.PercentToken,             BinaryOperator.Modulus,     TypeSymbol.Float),

        // Bitwise operations
        new IRBinaryOperator(SyntaxKind.TildeToken,               BinaryOperator.BitwiseNot,  TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.AmpersandToken,           BinaryOperator.BitwiseAnd,  TypeSymbol.Int), 
        new IRBinaryOperator(SyntaxKind.PipeToken,                BinaryOperator.BitwiseOr,   TypeSymbol.Int), 
        new IRBinaryOperator(SyntaxKind.CircumflexToken,          BinaryOperator.BitwiseXor,  TypeSymbol.Int),
        new IRBinaryOperator(SyntaxKind.DoubleLeftArrowToken,     BinaryOperator.BitshiftLeft,   TypeSymbol.Int), 
        new IRBinaryOperator(SyntaxKind.DoubleRightArrowToken,    BinaryOperator.BitshiftRight,  TypeSymbol.Int), 

        // Integer Comparisions
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperator.Equals,                  TypeSymbol.Int, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperator.NotEquals,               TypeSymbol.Int, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperator.GreaterThan,             TypeSymbol.Int, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperator.GreaterThanEqualsTo,     TypeSymbol.Int, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperator.LesserThan,              TypeSymbol.Int, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperator.LesserThanEqualsTo,      TypeSymbol.Int, TypeSymbol.Bool),

        // Float Comparisions
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperator.Equals,                  TypeSymbol.Float, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperator.NotEquals,               TypeSymbol.Float, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperator.GreaterThan,             TypeSymbol.Float, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperator.GreaterThanEqualsTo,     TypeSymbol.Float, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperator.LesserThan,              TypeSymbol.Float, TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperator.LesserThanEqualsTo,      TypeSymbol.Float, TypeSymbol.Bool),

        // Logical operations
        new IRBinaryOperator(SyntaxKind.DoubleAmpersandToken,     BinaryOperator.LogicalAnd,  TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.DoublePipeToken,          BinaryOperator.LogicalOr,   TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperator.Equals,      TypeSymbol.Bool),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperator.NotEquals,   TypeSymbol.Bool),

        // String operations
        new IRBinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperator.Equals, TypeSymbol.String),
        new IRBinaryOperator(SyntaxKind.BangEqualsToken, BinaryOperator.NotEquals, TypeSymbol.String),
        new IRBinaryOperator(SyntaxKind.PlusToken, BinaryOperator.Add, TypeSymbol.String),
        new IRBinaryOperator(SyntaxKind.StarToken, BinaryOperator.Multiply, TypeSymbol.String, TypeSymbol.Int, TypeSymbol.String),
    ];
}