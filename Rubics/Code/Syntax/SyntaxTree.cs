
namespace Rubics.Code.Syntax;

public abstract class SyntaxNode {
    public abstract SyntaxKind Kind { get; }
    public abstract IEnumerable<SyntaxNode> Children();
}

public sealed class SyntaxTree(SyntaxExpression root, Token endOfFileToken, Diagnostics diagnostics) {
    
    public static SyntaxTree Parse(string source) {
        var parser = new Parser(source);
        return parser.Parse();
    }
    
    public SyntaxExpression Root { get; } = root;
    public Token EndOfFileToken { get; } = endOfFileToken;
    public Diagnostics Diagnostics { get; } = diagnostics;
}