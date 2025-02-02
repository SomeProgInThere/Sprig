
using Rubics.Code.Source;

namespace Rubics.Code.Syntax;

public sealed class SyntaxTree(SourceText sourceText, Expression root, Token endOfFileToken, Diagnostics diagnostics) {
    
    public static SyntaxTree Parse(string source) {
        var sourceText = SourceText.FromString(source);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(SourceText sourceText) {
        var parser = new Parser(sourceText);
        return parser.Parse();
    }
    
    public static IEnumerable<Token> ParseTokens(string source) {
        var sourceText = SourceText.FromString(source);
        return ParseTokens(sourceText);
    }

    public static IEnumerable<Token> ParseTokens(SourceText source) {
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
    public SourceText SourceText { get; } = sourceText;
}