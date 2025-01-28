
namespace Rubics.Code.Syntax;

public enum SyntaxKind {
    // Tokens
    WhitespaceToken,
    EndOfFileToken,
    BadToken,
    IdentifierToken,

    NumberToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    PercentToken,
    BangToken,
    EqualsToken,
    DoubleAmpersandToken,
    DoublePipeToken,
    DoubleEqualsToken,
    BangEqualsToken,
    OpenParenthesisToken,
    ClosedParenthesisToken,

    // Keywords
    TrueKeyword,
    FalseKeyword,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
}

internal static class SyntaxKindExtensions {
    public static int UnaryOperatorPrecedence(this SyntaxKind kind) {
        return kind switch {
            SyntaxKind.PlusToken or SyntaxKind.MinusToken or SyntaxKind.BangToken => 6,
            _ => 0,
        };
    }

    public static int BinaryOperatorPrecedence(this SyntaxKind kind) {
        return kind switch {
            SyntaxKind.StarToken or SyntaxKind.SlashToken or SyntaxKind.PercentToken => 5,
            SyntaxKind.PlusToken or SyntaxKind.MinusToken => 4,
            SyntaxKind.DoubleEqualsToken or SyntaxKind.BangEqualsToken => 3,
            SyntaxKind.DoubleAmpersandToken => 2,
            SyntaxKind.DoublePipeToken => 1,
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
}
