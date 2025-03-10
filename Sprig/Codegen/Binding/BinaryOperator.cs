using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Binding;

internal enum BinaryOperatorKind {
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

internal sealed class BoundBinaryOperator(
    SyntaxKind syntaxKind, 
    BinaryOperatorKind kind, 
    TypeSymbol leftType, 
    TypeSymbol rightType, 
    TypeSymbol resultType
) {

    public BoundBinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind operatorKind, TypeSymbol type, TypeSymbol resultType)
        : this(syntaxKind, operatorKind, type, type, resultType) {}

    public BoundBinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind operatorKind, TypeSymbol type)
        : this(syntaxKind, operatorKind, type, type, type) {}

    public static BoundBinaryOperator? Bind(SyntaxKind kind, TypeSymbol leftType, TypeSymbol rightType) {
        foreach (var op in operators) {
            if (op.SyntaxKind == kind && op.LeftType == leftType && op.RightType == rightType) {
                return op;
            }
        }

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public BinaryOperatorKind Kind { get; } = kind;
    public TypeSymbol LeftType { get; } = leftType;
    public TypeSymbol RightType { get; } = rightType;
    public TypeSymbol ResultType { get; } = resultType;

    private static readonly BoundBinaryOperator[] operators = [
        // Interget operations
        new BoundBinaryOperator(SyntaxKind.PlusToken,                BinaryOperatorKind.Add,         TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.MinusToken,               BinaryOperatorKind.Substact,    TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.StarToken,                BinaryOperatorKind.Multiply,    TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.SlashToken,               BinaryOperatorKind.Divide,      TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.PercentToken,             BinaryOperatorKind.Modulus,     TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.DoubleSlashToken,         BinaryOperatorKind.Remainder,   TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.DoubleStarToken,          BinaryOperatorKind.RaisePower,  TypeSymbol.Int),
        
        // Float operations
        new BoundBinaryOperator(SyntaxKind.PlusToken,                BinaryOperatorKind.Add,         TypeSymbol.Float),
        new BoundBinaryOperator(SyntaxKind.MinusToken,               BinaryOperatorKind.Substact,    TypeSymbol.Float),
        new BoundBinaryOperator(SyntaxKind.StarToken,                BinaryOperatorKind.Multiply,    TypeSymbol.Float),
        new BoundBinaryOperator(SyntaxKind.SlashToken,               BinaryOperatorKind.Divide,      TypeSymbol.Float),
        new BoundBinaryOperator(SyntaxKind.PercentToken,             BinaryOperatorKind.Modulus,     TypeSymbol.Float),

        // Bitwise operations
        new BoundBinaryOperator(SyntaxKind.TildeToken,               BinaryOperatorKind.BitwiseNot,  TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.AmpersandToken,           BinaryOperatorKind.BitwiseAnd,  TypeSymbol.Int), 
        new BoundBinaryOperator(SyntaxKind.PipeToken,                BinaryOperatorKind.BitwiseOr,   TypeSymbol.Int), 
        new BoundBinaryOperator(SyntaxKind.CircumflexToken,          BinaryOperatorKind.BitwiseXor,  TypeSymbol.Int),
        new BoundBinaryOperator(SyntaxKind.DoubleLeftArrowToken,     BinaryOperatorKind.BitshiftLeft,   TypeSymbol.Int), 
        new BoundBinaryOperator(SyntaxKind.DoubleRightArrowToken,    BinaryOperatorKind.BitshiftRight,  TypeSymbol.Int), 

        // Integer Comparisions
        new BoundBinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,                  TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,               TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperatorKind.GreaterThan,             TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperatorKind.GreaterThanEqualsTo,     TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperatorKind.LesserThan,              TypeSymbol.Int, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperatorKind.LesserThanEqualsTo,      TypeSymbol.Int, TypeSymbol.Bool),

        // Float Comparisions
        new BoundBinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,                  TypeSymbol.Float, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,               TypeSymbol.Float, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperatorKind.GreaterThan,             TypeSymbol.Float, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperatorKind.GreaterThanEqualsTo,     TypeSymbol.Float, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperatorKind.LesserThan,              TypeSymbol.Float, TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperatorKind.LesserThanEqualsTo,      TypeSymbol.Float, TypeSymbol.Bool),

        // Logical operations
        new BoundBinaryOperator(SyntaxKind.DoubleAmpersandToken,     BinaryOperatorKind.LogicalAnd,  TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.DoublePipeToken,          BinaryOperatorKind.LogicalOr,   TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,      TypeSymbol.Bool),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,   TypeSymbol.Bool),

        // String operations
        new BoundBinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperatorKind.Equals, TypeSymbol.String),
        new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BinaryOperatorKind.NotEquals, TypeSymbol.String),
        new BoundBinaryOperator(SyntaxKind.PlusToken, BinaryOperatorKind.Add, TypeSymbol.String),
        new BoundBinaryOperator(SyntaxKind.StarToken, BinaryOperatorKind.Multiply, TypeSymbol.String, TypeSymbol.Int, TypeSymbol.String),
    ];
}