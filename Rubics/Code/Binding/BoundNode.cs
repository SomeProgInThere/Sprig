
namespace Rubics.Code.Binding;

internal enum BoundKind {
    LiteralExpression,
    UnaryExpression,
    BinaryExpression,
}

internal abstract class BoundNode {
    public abstract BoundKind Kind { get; }
}
