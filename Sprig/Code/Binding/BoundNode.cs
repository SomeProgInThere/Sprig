
using System.Reflection;

namespace Sprig.Code.Binding;

internal abstract class BoundNode {
    public abstract BoundNodeKind Kind { get; }

    public IEnumerable<BoundNode> Children() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties) {
            if (typeof(BoundNode).IsAssignableFrom(property.PropertyType)) {
                if (property.GetValue(this) is BoundNode child)
                    if (child != null)
                        yield return child;
            }

            else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType)){
                if (property.GetValue(this) is IEnumerable<BoundNode> children) {
                    foreach (var child in children)
                        if (child != null)
                            yield return child;
                }
            }
        }
    }

    public IEnumerable<(string Name, object Value)> Properties() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties) {
            if (property.Name == nameof(Kind) || property.Name == nameof(BoundBinaryExpression.Operator))
                continue;

            if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) 
                || typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                continue;

            var value = property.GetValue(this);
            if (value != null)
                yield return (property.Name, value);
        }
    }

    public void WriteTo(TextWriter writer) => BoundNodeExtensions.PrettyPrint(writer, this);

    public override string ToString() {
        using var writer = new StringWriter();
        WriteTo(writer);
        
        return writer.ToString();
    }
}