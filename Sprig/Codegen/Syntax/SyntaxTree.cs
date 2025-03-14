using System.Collections.Immutable;

using Sprig.Codegen.Text;

namespace Sprig.Codegen.Syntax;

public sealed class SyntaxTree {
    
    public static SyntaxTree Load(string fileName) {
        var source = File.ReadAllText(fileName);
        var sourceText = SourceText.FromString(source, fileName);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(string text) {
        var sourceText = SourceText.FromString(text);
        return Parse(sourceText);
    }

    public static SyntaxTree Parse(SourceText source) => new(source, Parse);

    public static IEnumerable<SyntaxToken> ParseTokens(string text) {
        var sourceText = SourceText.FromString(text);
        return ParseTokens(sourceText, out _);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<DiagnosticMessage> diagnostics) {
        var tokens = new List<SyntaxToken>();
        
        void ParseTokens(
            SyntaxTree syntaxTree, 
            out CompilationUnit root, 
            out ImmutableArray<DiagnosticMessage> diagnostics
        ) {            
            var lexer = new Lexer(syntaxTree);
            while (true) {
             
                var token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken) {
                    root = new CompilationUnit(syntaxTree, [], token);
                    break;
                }    
                tokens.Add(token);
            }
            diagnostics = [..lexer.Diagnostics];
        }

        var syntaxTree = new SyntaxTree(source, ParseTokens);
        diagnostics = syntaxTree.Diagnostics;
        return [..tokens];
    }

    private SyntaxTree(SourceText source, ParseHandler handler) {
        Source = source;
        handler(this, out var root, out var diagnostics);
    
        Root = root;
        Diagnostics = diagnostics;
    }

    private static void Parse(
        SyntaxTree syntaxTree, 
        out CompilationUnit root, 
        out ImmutableArray<DiagnosticMessage> diagnostics
    ) {
        var parser = new Parser(syntaxTree);
        root = parser.ParseCompilationUnit();
        diagnostics = [..parser.Diagnostics];
    }

    private delegate void ParseHandler(
        SyntaxTree syntaxTree, 
        out CompilationUnit root, 
        out ImmutableArray<DiagnosticMessage> diagnostics
    );

    public SourceText Source { get; }
    public CompilationUnit Root { get; }
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; }
}