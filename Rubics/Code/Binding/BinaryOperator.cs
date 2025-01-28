
using Rubics.Code.Syntax;

namespace Rubics.Code.Binding;

internal enum BinaryOperatorKind {
    Add,
    Substact,
    Multiply,
    Divide,
    Modulus,
    LogicalAnd,
    LogicalOr,
    Equals,
    NotEquals,
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
        new BinaryOperator(SyntaxKind.PlusToken,            BinaryOperatorKind.Add,         typeof(int)),
        new BinaryOperator(SyntaxKind.MinusToken,           BinaryOperatorKind.Substact,    typeof(int)),
        new BinaryOperator(SyntaxKind.StarToken,            BinaryOperatorKind.Multiply,    typeof(int)),
        new BinaryOperator(SyntaxKind.SlashToken,           BinaryOperatorKind.Divide,      typeof(int)),
        new BinaryOperator(SyntaxKind.PercentToken,         BinaryOperatorKind.Modulus,     typeof(int)),

        new BinaryOperator(SyntaxKind.DoubleEqualsToken,    BinaryOperatorKind.Equals,      typeof(int), typeof(bool)),
        new BinaryOperator(SyntaxKind.BangEqualsToken,      BinaryOperatorKind.NotEquals,   typeof(int), typeof(bool)),

        new BinaryOperator(SyntaxKind.DoubleAmpersandToken, BinaryOperatorKind.LogicalAnd,  typeof(bool)),
        new BinaryOperator(SyntaxKind.DoublePipeToken,      BinaryOperatorKind.LogicalOr,   typeof(bool)),
        new BinaryOperator(SyntaxKind.DoubleEqualsToken,    BinaryOperatorKind.Equals,      typeof(bool)),
        new BinaryOperator(SyntaxKind.BangEqualsToken,      BinaryOperatorKind.NotEquals,   typeof(bool)),
    ];
}