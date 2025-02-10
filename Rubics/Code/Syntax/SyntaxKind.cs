
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
    TildeToken,
    AmpersandToken,
    PipeToken,
    CircumflexToken,
    LeftArrowToken,
    RightArrowToken,

    DoubleAmpersandToken,
    DoublePipeToken,
    DoubleEqualsToken,
    PlusEqualsToken,
    MinusEqualsToken,
    StarEqualsToken,
    SlashEqualsToken,
    PercentEqualsToken,
    BangEqualsToken,
    AmpersandEqualsToken,
    PipeEqualsToken,
    TildeEqualsToken,
    CircumflexEqualsToken,
    LeftArrowEqualsToken,
    RightArrowEqualsToken,

    OpenParenthesisToken,
    ClosedParenthesisToken,
    OpenBraceToken,
    ClosedBraceToken,

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

    // Statements
    BlockStatment,
    ExpressionStatement,
}