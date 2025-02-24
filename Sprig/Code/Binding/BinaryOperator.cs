
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

internal sealed class BinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind kind, Type leftType, Type rightType, Type resultType) {

    public BinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind operatorKind, Type type, Type resultType)
        : this(syntaxKind, operatorKind, type, type, resultType) {}

    public BinaryOperator(SyntaxKind syntaxKind, BinaryOperatorKind operatorKind, Type type)
        : this(syntaxKind, operatorKind, type, type, type) {}

    public static BinaryOperator? Bind(SyntaxKind kind, Type leftType, Type rightType) {
        foreach (var op in operators) {
            if (op.SyntaxKind == kind && op.LeftType == leftType && op.RightType == rightType) {
                return op;
            }
        }

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public BinaryOperatorKind Kind { get; } = kind;
    public Type LeftType { get; } = leftType;
    public Type RightType { get; } = rightType;
    public Type ResultType { get; } = resultType;

    private static readonly BinaryOperator[] operators = [
        new BinaryOperator(SyntaxKind.PlusToken,                BinaryOperatorKind.Add,         typeof(int)),
        new BinaryOperator(SyntaxKind.MinusToken,               BinaryOperatorKind.Substact,    typeof(int)),
        new BinaryOperator(SyntaxKind.StarToken,                BinaryOperatorKind.Multiply,    typeof(int)),
        new BinaryOperator(SyntaxKind.SlashToken,               BinaryOperatorKind.Divide,      typeof(int)),
        new BinaryOperator(SyntaxKind.PercentToken,             BinaryOperatorKind.Modulus,     typeof(int)),
        new BinaryOperator(SyntaxKind.DoubleSlashToken,         BinaryOperatorKind.Remainder,   typeof(int)),
        new BinaryOperator(SyntaxKind.DoubleStarToken,          BinaryOperatorKind.RaisePower,  typeof(int)),
        
        new BinaryOperator(SyntaxKind.TildeToken,               BinaryOperatorKind.BitwiseNot,  typeof(int)),
        new BinaryOperator(SyntaxKind.AmpersandToken,           BinaryOperatorKind.BitwiseAnd,  typeof(int)), 
        new BinaryOperator(SyntaxKind.PipeToken,                BinaryOperatorKind.BitwiseOr,   typeof(int)), 
        new BinaryOperator(SyntaxKind.CircumflexToken,          BinaryOperatorKind.BitwiseXor,  typeof(int)),
         
        new BinaryOperator(SyntaxKind.DoubleLeftArrowToken,     BinaryOperatorKind.BitshiftLeft,   typeof(int)), 
        new BinaryOperator(SyntaxKind.DoubleRightArrowToken,    BinaryOperatorKind.BitshiftRight,  typeof(int)), 

        new BinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,                  typeof(int), typeof(bool)),
        new BinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,               typeof(int), typeof(bool)),
        new BinaryOperator(SyntaxKind.LeftArrowToken,           BinaryOperatorKind.GreaterThan,             typeof(int), typeof(bool)),
        new BinaryOperator(SyntaxKind.LeftArrowEqualsToken,     BinaryOperatorKind.GreaterThanEqualsTo,     typeof(int), typeof(bool)),
        new BinaryOperator(SyntaxKind.RightArrowToken,          BinaryOperatorKind.LesserThan,              typeof(int), typeof(bool)),
        new BinaryOperator(SyntaxKind.RightArrowEqualsToken,    BinaryOperatorKind.LesserThanEqualsTo,      typeof(int), typeof(bool)),

        new BinaryOperator(SyntaxKind.DoubleAmpersandToken,     BinaryOperatorKind.LogicalAnd,  typeof(bool)),
        new BinaryOperator(SyntaxKind.DoublePipeToken,          BinaryOperatorKind.LogicalOr,   typeof(bool)),
        new BinaryOperator(SyntaxKind.DoubleEqualsToken,        BinaryOperatorKind.Equals,      typeof(bool)),
        new BinaryOperator(SyntaxKind.BangEqualsToken,          BinaryOperatorKind.NotEquals,   typeof(bool)),
    ];
}