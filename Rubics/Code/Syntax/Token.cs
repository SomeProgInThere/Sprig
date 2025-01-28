
namespace Rubics.Code.Syntax;

public sealed class Token(SyntaxKind kind, int position, string literal, object? value = null) 
    : SyntaxNode {

    public Token(SyntaxKind kind, ref int position, int offset, string literal, object? value = null) 
        : this(kind, position, literal, offset) {
            position += offset;
        }

    public int Position { get; } = position;
    public string Literal { get; } = literal;
    public object? Value { get; } = value;
    public TextSpan Span => new(Position, Literal.Length);

    public override SyntaxKind Kind { get; } = kind;
    public override IEnumerable<SyntaxNode> Children() => [];
}