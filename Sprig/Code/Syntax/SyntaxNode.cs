using System.Reflection;

using Sprig.Code.Source;

namespace Sprig.Code.Syntax;

public abstract class SyntaxNode {
    public abstract SyntaxKind Kind { get; }
    
    public virtual TextSpan Span {
        get {
            var first = Children().First().Span;
            var last = Children().Last().Span;
            return TextSpan.CreateFromBounds(first.Start, last.End);
        }
    }

    public IEnumerable<SyntaxNode> Children() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties) {
            if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType)) {
                if (property.GetValue(this) is SyntaxNode child)
                    if (child != null)
                        yield return child;
            }

            else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType)){
                if (property.GetValue(this) is IEnumerable<SyntaxNode> children) {
                    foreach (var child in children)
                        if (child != null)
                            yield return child;
                }
            }
        }
    }

    public SyntaxToken LastToken() {
        if (this is SyntaxToken token)
            return token;

        return Children().Last().LastToken();
    }

    public void WriteTo(TextWriter writer) {
        writer.WriteLine("ParseTree");
        SyntaxNodeExtensions.PrettyPrint(writer, this);
        writer.WriteLine();
    }

    public override string ToString() {
        using var writer = new StringWriter();
        WriteTo(writer);
        
        return writer.ToString();
    }
}