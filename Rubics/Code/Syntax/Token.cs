
namespace Rubics.Code.Syntax;

public sealed class Token(SyntaxKind kind, int position, string literal, object? value = null) 
    : SyntaxNode {

    public int Position { get; } = position;
    public string Literal { get; } = literal;
    public object? Value { get; } = value;

    public override SyntaxKind Kind { get; } = kind;
    public override IEnumerable<SyntaxNode> Children() => [];
}