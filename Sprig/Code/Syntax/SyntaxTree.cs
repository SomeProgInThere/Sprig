using System.Collections.Immutable;

using Sprig.Code.Source;

namespace Sprig.Code.Syntax;

public sealed class SyntaxTree {

    public static SyntaxTree Parse(string source) {
        var sourceText = SourceText.FromString(source);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(SourceText sourceText) => new(sourceText);
    
    public static IEnumerable<SyntaxToken> ParseTokens(string source) {
        var sourceText = SourceText.FromString(source);
        return ParseTokens(sourceText);
    }

    public static IEnumerable<SyntaxToken> ParseTokens(SourceText source) {
        var lexer = new Lexer(source);
        while (true) {
            var token = lexer.Lex();
            if (token.Kind == SyntaxKind.EndOfFileToken) 
                break;
        
            yield return token;
        }
    }

    private SyntaxTree(SourceText sourceText) {
        var parser = new Parser(sourceText);
        
        SourceText = sourceText;
        Root = parser.ParseCompilationUnit();
        Diagnostics = [.. parser.Diagnostics];
    }

    public SourceText SourceText { get; }
    public CompilationUnit Root { get; }
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; }
}

public class CompilationUnit(Statement statement, SyntaxToken endOfFileToken) 
    : SyntaxNode {

    public Statement Statement { get; } = statement;
    public SyntaxToken EndOfFile { get; } = endOfFileToken;

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}