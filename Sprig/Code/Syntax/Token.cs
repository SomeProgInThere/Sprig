using Sprig.Code.Source;

namespace Sprig.Code.Syntax;

public sealed class Token(SyntaxKind kind, int position, string literal, object? value = null) 
    : SyntaxNode {

    public int Position { get; } = position;
    public string Literal { get; } = literal;
    public object? Value { get; } = value;

    public override TextSpan Span => new(Position, Literal.Length);
    public override SyntaxKind Kind { get; } = kind;
}

public static class TokenExtensions {
    public static bool IsNull(this Token token) => token.Literal == "\0";
}