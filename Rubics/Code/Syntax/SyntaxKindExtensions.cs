
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
            SyntaxKind.PlusToken => "+",
            SyntaxKind.PlusEqualsToken => "+=",
            SyntaxKind.MinusToken => "-",
            SyntaxKind.MinusEqualsToken => "-=",
            SyntaxKind.StarToken => "*",
            SyntaxKind.StarEqualsToken => "*=",
            SyntaxKind.SlashToken => "/",
            SyntaxKind.SlashEqualsToken => "/=",
            SyntaxKind.PercentToken => "%",
            SyntaxKind.PercentEqualsToken => "%=",
            SyntaxKind.BangToken => "!",
            SyntaxKind.BangEqualsToken => "!=",
            SyntaxKind.TildeToken => "~",
            SyntaxKind.TildeEqualsToken => "~=",
            SyntaxKind.EqualsToken => "=",
            SyntaxKind.DoubleEqualsToken => "==",
            SyntaxKind.AmpersandToken => "&",
            SyntaxKind.DoubleAmpersandToken => "&&",
            SyntaxKind.AmpersandEqualsToken => "&=",
            SyntaxKind.PipeToken => "|",
            SyntaxKind.DoublePipeToken => "||",
            SyntaxKind.PipeEqualsToken => "|=",
            SyntaxKind.CircumflexToken => "^",
            SyntaxKind.CircumflexEqualsToken => "^=",
            SyntaxKind.OpenParenthesisToken => "(",
            SyntaxKind.ClosedParenthesisToken => ")",
            _ => null,
        };
    }
}