
using Rubics.Code.Syntax;

namespace Rubics.Code.Binding;

internal enum UnaryOperatorKind {
    Identity,
    Negetion,
    LogicalNot,
}

internal sealed class UnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind kind, Type operandType, Type resultType) {

    public UnaryOperator(SyntaxKind syntaxKind, UnaryOperatorKind operatorKind, Type operandType)
        : this(syntaxKind, operatorKind, operandType, operandType) {}

    public static UnaryOperator? Bind(SyntaxKind kind, Type operandType) {
        foreach (var op in operators) {
            if (op.SyntaxKind == kind && op.OperandType == operandType) {
                return op;
            }
        }

        return null;
    }

    public SyntaxKind SyntaxKind { get; } = syntaxKind;
    public UnaryOperatorKind Kind { get; } = kind;
    public Type OperandType { get; } = operandType;
    public Type ResultType { get; } = resultType;

    private static readonly UnaryOperator[] operators = [
        new UnaryOperator(SyntaxKind.BangToken, UnaryOperatorKind.LogicalNot, typeof(bool)),
        new UnaryOperator(SyntaxKind.PlusToken, UnaryOperatorKind.Identity, typeof(int)),
        new UnaryOperator(SyntaxKind.MinusToken, UnaryOperatorKind.Negetion, typeof(int)),
    ];
}

internal sealed class BoundUnaryExpression(BoundExpression operand, UnaryOperator op)
    : BoundExpression {

    public UnaryOperator Op { get; } = op;
    public BoundExpression Operand { get; } = operand;

    public override BoundKind Kind => BoundKind.UnaryExpression;
    public override Type Type => Operand.Type;
}
