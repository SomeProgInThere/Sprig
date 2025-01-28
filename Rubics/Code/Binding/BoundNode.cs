
namespace Rubics.Code.Binding;

internal enum BoundKind {
    LiteralExpression,
    UnaryExpression,
    VariableExpression,
    BinaryExpression,
    AssignmentExpression,
}

internal abstract class BoundNode {
    public abstract BoundKind Kind { get; }
}
