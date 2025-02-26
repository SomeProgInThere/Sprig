using Sprig.Code.Source;

namespace Sprig.Code.Syntax;

public sealed class SyntaxToken(SyntaxKind kind, int position, string? literal, object? value = null) 
    : SyntaxNode {

    public int Position { get; } = position;
    public string? Literal { get; } = literal;
    public object? Value { get; } = value;
    public bool IsMissing => Literal is null;

    public string LiteralOrEmpty => Literal ?? "";

    public override TextSpan Span => new(Position, Literal?.Length ?? 0);
    public override SyntaxKind Kind { get; } = kind;
}