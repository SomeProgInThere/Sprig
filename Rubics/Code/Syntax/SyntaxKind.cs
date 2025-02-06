
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
    
    // Nodes
    CompilationUnit,
}