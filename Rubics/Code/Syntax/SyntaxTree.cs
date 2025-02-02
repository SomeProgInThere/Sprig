
namespace Rubics.Code.Syntax;

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