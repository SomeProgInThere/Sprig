using Sprig.Codegen.Text;

namespace Sprig.Codegen.Syntax;

public sealed class SyntaxToken(
    SyntaxTree syntaxTree, 
    SyntaxKind kind, 
    int position, 
    string text, 
    object? value = null
) 
    : SyntaxNode(syntaxTree) {

    public int Position { get; } = position;
    public string Text { get; } = text;
    public object? Value { get; } = value;

    public bool IsMissing() => Text == "";
    public bool IsTrivia() => Kind switch {
        SyntaxKind.BadTokenTrivia or 
        SyntaxKind.WhitespaceTrivia or
        SyntaxKind.SinglelineCommentTrivia or 
        SyntaxKind.MultilineCommentTrivia => true,
        _ => false
    };

    public override TextSpan Span => new(Position, Text?.Length ?? 0);
    public override SyntaxKind Kind { get; } = kind;
}