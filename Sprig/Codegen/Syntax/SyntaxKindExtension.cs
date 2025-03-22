namespace Sprig.Codegen.Syntax;

internal static class SyntaxKindExtension {
    
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
            SyntaxKind.PipeToken or
            SyntaxKind.DoubleLeftArrowToken or
            SyntaxKind.DoubleRightArrowToken => 6,

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

    public static SyntaxKind KeywordKind(this string literal) {
        return literal switch {
            "true"     => SyntaxKind.TrueKeyword,
            "false"    => SyntaxKind.FalseKeyword,
            "if"       => SyntaxKind.IfKeyword,
            "else"     => SyntaxKind.ElseKeyword,
            "while"    => SyntaxKind.WhileKeyword,
            "do"       => SyntaxKind.DoKeyword,
            "for"      => SyntaxKind.ForKeyword,
            "break"    => SyntaxKind.BreakKeyword,
            "continue" => SyntaxKind.ContinueKeyword,
            "return"   => SyntaxKind.ReturnKeyword,
            "in"       => SyntaxKind.InKeyword,
            "func"     => SyntaxKind.FuncKeyword,
            "let"      => SyntaxKind.LetKeyword,
            "var"      => SyntaxKind.VarKeyword,

            _ => SyntaxKind.IdentifierToken,
        };
    }

    public static string? Text(this SyntaxKind kind) {
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
            SyntaxKind.LeftArrowToken           => ">",
            SyntaxKind.RightArrowToken          => "<",
            SyntaxKind.CommaToken               => ",",
            SyntaxKind.ColonToken               => ":",

            SyntaxKind.BangEqualsToken          => "!=",
            SyntaxKind.DoubleEqualsToken        => "==",
            SyntaxKind.LeftArrowEqualsToken     => ">=",
            SyntaxKind.RightArrowEqualsToken    => "<=",
            SyntaxKind.DoubleAmpersandToken     => "&&",
            SyntaxKind.DoublePipeToken          => "||",
            SyntaxKind.DoubleLeftArrowToken     => ">>",
            SyntaxKind.DoubleRightArrowToken    => "<<",
            SyntaxKind.DoubleDotToken           => "..", 
            
            SyntaxKind.TrueKeyword     => "true",
            SyntaxKind.FalseKeyword    => "false",
            SyntaxKind.IfKeyword       => "if",
            SyntaxKind.WhileKeyword    => "while",
            SyntaxKind.DoKeyword       => "do",
            SyntaxKind.ForKeyword      => "for",
            SyntaxKind.InKeyword       => "in",
            SyntaxKind.FuncKeyword     => "func",
            SyntaxKind.ElseKeyword     => "else",
            SyntaxKind.BreakKeyword    => "break",
            SyntaxKind.ContinueKeyword => "continue",
            SyntaxKind.ReturnKeyword   => "return",
            SyntaxKind.LetKeyword      => "let",
            SyntaxKind.VarKeyword      => "var",

            _ => null,
        };
    }
}