using Sprig.Code.Symbols;
using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

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

internal sealed class BinaryOperator(
    SyntaxKind syntaxKind, 
    BinaryOperatorKind kind, 
    TypeSymbol leftType, 
    TypeSymbol rightType, 
    TypeSymbol resultType
) {

    public BinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind operatorKind, TypeSymbol type, TypeSymbol resultType)
        : this(syntaxKind, operatorKind, type, type, resultType) {}

    public BinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind operatorKind, TypeSymbol type)
        : this(syntaxKind, operatorKind, type, type, type) {}

    public static BinaryOperator? Bind(SyntaxKind kind, TypeSymbol leftType, TypeSymbol rightType) {
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

    private static readonly BinaryOperator[] operators = [
        // Interget operations
        new BinaryOperator(SyntaxKind.PlusToken,                BinaryOperatorKind.Add,         TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.MinusToken,               BinaryOperatorKind.Substact,    TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.StarToken,                BinaryOperatorKind.Multiply,    TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.SlashToken,               BinaryOperatorKind.Divide,      TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.PercentToken,             BinaryOperatorKind.Modulus,     TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.DoubleSlashToken,         BinaryOperatorKind.Remainder,   TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.DoubleStarToken,          BinaryOperatorKind.RaisePower,  TypeSymbol.Int),
        
        // Float operations
        new BinaryOperator(SyntaxKind.PlusToken,                BinaryOperatorKind.Add,         TypeSymbol.Float),
        new BinaryOperator(SyntaxKind.MinusToken,               BinaryOperatorKind.Substact,    TypeSymbol.Float),
        new BinaryOperator(SyntaxKind.StarToken,                BinaryOperatorKind.Multiply,    TypeSymbol.Float),
        new BinaryOperator(SyntaxKind.SlashToken,               BinaryOperatorKind.Divide,      TypeSymbol.Float),
        new BinaryOperator(SyntaxKind.PercentToken,             BinaryOperatorKind.Modulus,     TypeSymbol.Float),

        // Bitwise operations
        new BinaryOperator(SyntaxKind.TildeToken,               BinaryOperatorKind.BitwiseNot,  TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.AmpersandToken,           BinaryOperatorKind.BitwiseAnd,  TypeSymbol.Int), 
        new BinaryOperator(SyntaxKind.PipeToken,                BinaryOperatorKind.BitwiseOr,   TypeSymbol.Int), 
        new BinaryOperator(SyntaxKind.CircumflexToken,          BinaryOperatorKind.BitwiseXor,  TypeSymbol.Int),
        new BinaryOperator(SyntaxKind.DoubleLeftArrowToken,     BinaryOperatorKind.BitshiftLeft,   TypeSymbol.Int), 
        new BinaryOperator(SyntaxKind.DoubleRightArrowToken,    BinaryOperatorKind.BitshiftRight,  TypeSymbol.Int), 

        // Integer Comparisions
        new BinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,                  TypeSymbol.Int, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,               TypeSymbol.Int, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperatorKind.GreaterThan,             TypeSymbol.Int, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperatorKind.GreaterThanEqualsTo,     TypeSymbol.Int, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperatorKind.LesserThan,              TypeSymbol.Int, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperatorKind.LesserThanEqualsTo,      TypeSymbol.Int, TypeSymbol.Bool),

        // Float Comparisions
        new BinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,                  TypeSymbol.Float, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,               TypeSymbol.Float, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperatorKind.GreaterThan,             TypeSymbol.Float, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperatorKind.GreaterThanEqualsTo,     TypeSymbol.Float, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperatorKind.LesserThan,              TypeSymbol.Float, TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperatorKind.LesserThanEqualsTo,      TypeSymbol.Float, TypeSymbol.Bool),

        // Logical operations
        new BinaryOperator(SyntaxKind.DoubleAmpersandToken,     BinaryOperatorKind.LogicalAnd,  TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.DoublePipeToken,          BinaryOperatorKind.LogicalOr,   TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,      TypeSymbol.Bool),
        new BinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,   TypeSymbol.Bool),

        // String operations
        new BinaryOperator(SyntaxKind.DoubleEqualsToken, BinaryOperatorKind.Equals, TypeSymbol.String),
        new BinaryOperator(SyntaxKind.BangEqualsToken, BinaryOperatorKind.NotEquals, TypeSymbol.String),
        new BinaryOperator(SyntaxKind.PlusToken, BinaryOperatorKind.Add, TypeSymbol.String),
        new BinaryOperator(SyntaxKind.StarToken, BinaryOperatorKind.Multiply, TypeSymbol.String, TypeSymbol.Int, TypeSymbol.String),
    ];
}