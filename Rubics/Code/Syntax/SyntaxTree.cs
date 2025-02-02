
using System.Reflection;

namespace Rubics.Code.Syntax;

public abstract class SyntaxNode {
    public abstract SyntaxKind Kind { get; }
    
    public IEnumerable<SyntaxNode> Children() {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties) {
            if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType)) {
                if (property.GetValue(this) is SyntaxNode child)
                    yield return child;
            }

            else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType)){
                if (property.GetValue(this) is IEnumerable<SyntaxNode> children) {
                    foreach (var child in children)
                        yield return child;
                }
            }
        }
    }
}

public sealed class SyntaxTree(Expression root, Token endOfFileToken, Diagnostics diagnostics) {
    
    public static SyntaxTree Parse(string source) {
        var parser = new Parser(source);
        return parser.Parse();
    }
    
    public static IEnumerable<Token> ParseTokens(string source) {
        var lexer = new Lexer(source);
        while (true) {
            var token = lexer.Lex();
            if (token.Kind == SyntaxKind.EndOfFileToken) 
                break;
        
            yield return token;
        }
    }

    public Expression Root { get; } = root;
    public Token EndOfFileToken { get; } = endOfFileToken;
    public Diagnostics Diagnostics { get; } = diagnostics;
}