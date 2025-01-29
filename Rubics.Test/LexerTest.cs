
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class LexerTest {

    [Theory]
    [MemberData(nameof(TokensData))]
    public void LexingTokens(KindData data) {
        var tokens = SyntaxTree.ParseTokens(data.Literal);
        var token = Assert.Single(tokens);

        Assert.Equal(data.Kind, token.Kind);
        Assert.Equal(data.Literal, token.Literal);
    }

    [Theory]
    [MemberData(nameof(TokenPairsData))]
    public void LexingTokenPairs(KindData data1, KindData data2) {
        var literals = data1.Literal + data2.Literal;
        var tokens = SyntaxTree.ParseTokens(literals).ToArray();

        Assert.Equal(2, tokens.Length);
        Assert.Equal(tokens[0].Kind, data1.Kind);
        Assert.Equal(tokens[0].Literal, data1.Literal);
        
        Assert.Equal(tokens[1].Kind, data2.Kind);
        Assert.Equal(tokens[1].Literal, data2.Literal);
    }

    [Theory]
    [MemberData(nameof(TokenPairsWithSeperatorData))]
    public void LexingTokenPairsWithSeperator(KindData data1, KindData data2, KindData seperator) {
        var literals = data1.Literal + seperator.Literal + data2.Literal;
        var tokens = SyntaxTree.ParseTokens(literals).ToArray();

        Assert.Equal(3, tokens.Length);
        Assert.Equal(tokens[0].Kind, data1.Kind);
        Assert.Equal(tokens[0].Literal, data1.Literal);
        
        Assert.Equal(tokens[1].Kind, seperator.Kind);
        Assert.Equal(tokens[1].Literal, seperator.Literal);
        
        Assert.Equal(tokens[2].Kind, data2.Kind);
        Assert.Equal(tokens[2].Literal, data2.Literal);
    }

    public static IEnumerable<object[]> TokensData() {
        foreach (var data in Tokens().Concat(Seperators()))
            yield return new object[] { data };
    }

    public static IEnumerable<object[]> TokenPairsData() {
        foreach (var (data1, data2) in TokenPairs())
            yield return new object[] { data1, data2 };
    }

    public static IEnumerable<object[]> TokenPairsWithSeperatorData() {
        foreach (var (data1, data2, seperator) in TokenPairsWithSeperator())
            yield return new object[] { data1, data2, seperator };
    }

    private static IEnumerable<(KindData data1, KindData data2)> TokenPairs() {
        foreach (var t1 in Tokens()) {
            foreach (var t2 in Tokens()) {                
                
                if (!RequiredSeperator(t1.Kind, t2.Kind))
                    yield return (
                        new KindData(t1.Kind, t1.Literal), 
                        new KindData(t2.Kind, t2.Literal)
                    );
            }
        }
    }

    private static IEnumerable<(KindData data1, KindData data2, KindData seperator)> TokenPairsWithSeperator() {
        foreach (var t1 in Tokens()) {
            foreach (var t2 in Tokens()) {                
                
                if (!RequiredSeperator(t1.Kind, t2.Kind)) {
                    foreach (var seperator in Seperators())
                        yield return (
                            new KindData(t1.Kind, t1.Literal), 
                            new KindData(t2.Kind, t2.Literal), 
                            new KindData(seperator.Kind, seperator.Literal)
                        );
                }
            }
        }
    }

    private static bool RequiredSeperator(SyntaxKind k1, SyntaxKind k2) {
        var key1 = k1.ToString().EndsWith("Keyword");
        var key2 = k2.ToString().EndsWith("Keyword");
        
        if (k1 == SyntaxKind.IdentifierToken && k2 == SyntaxKind.IdentifierToken)
            return true;
        if (key1 && key2)
            return true;
        if (key1 && k2 == SyntaxKind.IdentifierToken)
            return true;
        if (k1 == SyntaxKind.IdentifierToken && key2)
            return true;
        if (k1 == SyntaxKind.NumberToken && k2 == SyntaxKind.NumberToken)
            return true;
        if (k1 == SyntaxKind.BangToken && k2 == SyntaxKind.EqualsToken)
            return true;
        if (k1 == SyntaxKind.BangToken && k2 == SyntaxKind.DoubleEqualsToken)
            return true;
        if (k1 == SyntaxKind.EqualsToken && k2 == SyntaxKind.EqualsToken)
            return true;
        if (k1 == SyntaxKind.EqualsToken && k2 == SyntaxKind.DoubleEqualsToken)
            return true;

        return false;
    }

    public readonly struct KindData(SyntaxKind kind, string literal) {
        public SyntaxKind Kind { get; } = kind;
        public string Literal { get; } = literal;
    }

    private static IEnumerable<KindData> Tokens() {
        return [
            new(SyntaxKind.PlusToken,              "+"),
            new(SyntaxKind.MinusToken,             "-"),
            new(SyntaxKind.StarToken,              "*"),
            new(SyntaxKind.SlashToken,             "/"),
            new(SyntaxKind.PercentToken,           "%"),
            new(SyntaxKind.BangToken,              "!"),
            new(SyntaxKind.EqualsToken,            "="),
            new(SyntaxKind.DoubleAmpersandToken,   "&&"),
            new(SyntaxKind.DoublePipeToken,        "||"),
            new(SyntaxKind.DoubleEqualsToken,      "=="),
            new(SyntaxKind.BangEqualsToken,        "!="),
            new(SyntaxKind.OpenParenthesisToken,   "("),
            new(SyntaxKind.ClosedParenthesisToken, ")"),
            
            new(SyntaxKind.TrueKeyword,  "true"),
            new(SyntaxKind.FalseKeyword, "false"),

            new(SyntaxKind.NumberToken,     "1"),
            new(SyntaxKind.NumberToken,     "123"),
            new(SyntaxKind.IdentifierToken, "a"),
            new(SyntaxKind.IdentifierToken, "abc"),
        ];
    }

    private static IEnumerable<KindData> Seperators() {
        return [
            new(SyntaxKind.WhitespaceToken, " "),
            new(SyntaxKind.WhitespaceToken, "  "),
            new(SyntaxKind.WhitespaceToken, "\r"),
            new(SyntaxKind.WhitespaceToken, "\n"),
            new(SyntaxKind.WhitespaceToken, "\r\n"),
        ];
    }
}