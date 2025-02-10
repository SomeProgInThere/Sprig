
namespace Rubics.Code.Syntax;

internal static class SyntaxKindExtensions {
    
    public static int UnaryOperatorPrecedence(this SyntaxKind kind) {
        return kind switch {
            SyntaxKind.PlusToken or 
            SyntaxKind.MinusToken or 
            SyntaxKind.BangToken or
            SyntaxKind.TildeToken => 7,
            _ => 0,
        };
    }

    public static int BinaryOperatorPrecedence(this SyntaxKind kind) {
        return kind switch {
            SyntaxKind.AmpersandToken or
            SyntaxKind.CircumflexToken or
            SyntaxKind.PipeToken => 6,

            SyntaxKind.StarToken or 
            SyntaxKind.SlashToken or 
            SyntaxKind.PercentToken => 5,
            
            SyntaxKind.PlusToken or 
            SyntaxKind.MinusToken => 4,
            
            SyntaxKind.DoubleEqualsToken or 
            SyntaxKind.BangEqualsToken => 3,
            
            SyntaxKind.DoubleAmpersandToken or
            SyntaxKind.DoublePipeToken => 2,

            SyntaxKind.LeftArrowToken or
            SyntaxKind.LeftArrowEqualsToken or
            SyntaxKind.RightArrowToken or
            SyntaxKind.RightArrowEqualsToken => 1,
            _ => 0,
        };
    }

    public static SyntaxKind GetKeywordKind(string literal) {
        return literal switch {
            "true"  => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            _ => SyntaxKind.IdentifierToken,
        };
    }

    public static string? GetLiteral(SyntaxKind kind) {
        return kind switch {
            SyntaxKind.PlusToken                => "+",
            SyntaxKind.MinusToken               => "-",
            SyntaxKind.StarToken                => "*",
            SyntaxKind.SlashToken               => "/",
            SyntaxKind.PercentToken             => "%",
            SyntaxKind.BangToken                => "!",
            SyntaxKind.TildeToken               => "~",
            SyntaxKind.EqualsToken              => "=",
            SyntaxKind.AmpersandToken           => "&",
            SyntaxKind.PipeToken                => "|",
            SyntaxKind.CircumflexToken          => "^",
            SyntaxKind.OpenParenthesisToken     => "(",
            SyntaxKind.ClosedParenthesisToken   => ")",
            SyntaxKind.OpenBraceToken           => "{",
            SyntaxKind.ClosedBraceToken         => "}",
            SyntaxKind.PlusEqualsToken          => "+=",
            SyntaxKind.MinusEqualsToken         => "-=",
            SyntaxKind.StarEqualsToken          => "*=",
            SyntaxKind.SlashEqualsToken         => "/=",
            SyntaxKind.PercentEqualsToken       => "%=",
            SyntaxKind.BangEqualsToken          => "!=",
            SyntaxKind.TildeEqualsToken         => "~=",
            SyntaxKind.DoubleEqualsToken        => "==",
            SyntaxKind.DoubleAmpersandToken     => "&&",
            SyntaxKind.AmpersandEqualsToken     => "&=",
            SyntaxKind.DoublePipeToken          => "||",
            SyntaxKind.PipeEqualsToken          => "|=",
            SyntaxKind.CircumflexEqualsToken    => "^=",
            _ => null,
        };
    }
}