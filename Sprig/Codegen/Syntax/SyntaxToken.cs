using Sprig.Codegen.Text;

namespace Sprig.Codegen.Syntax;

public sealed class SyntaxToken(
    SyntaxTree syntaxTree, 
    SyntaxKind kind, 
    int position, 
    string literal, 
    object? value = null
) 
    : SyntaxNode(syntaxTree) {

    public int Position { get; } = position;
    public string Literal { get; } = literal;
    public object? Value { get; } = value;
    public bool IsMissing => Literal == "";

    public override TextSpan Span => new(Position, Literal?.Length ?? 0);
    public override SyntaxKind Kind { get; } = kind;
}