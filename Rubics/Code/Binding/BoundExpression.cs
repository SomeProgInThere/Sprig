
namespace Rubics.Code.Binding;

internal abstract class BoundExpression : BoundNode {
    public abstract Type Type { get; }
}

internal sealed class BoundLiteralExpression(object value) 
    : BoundExpression {

    public object Value { get; } = value;

    public override BoundKind Kind => BoundKind.LiteralExpression;
    public override Type Type => Value.GetType();
}
