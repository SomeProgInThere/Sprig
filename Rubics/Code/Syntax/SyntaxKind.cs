
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
    DotToken,

    DoubleAmpersandToken,
    DoublePipeToken,
    DoubleEqualsToken,
    DoubleLeftArrowToken,
    DoubleRightArrowToken,
    DoubleDotToken,
    PlusEqualsToken,
    MinusEqualsToken,
    StarEqualsToken,
    SlashEqualsToken,
    PercentEqualsToken,
    BangEqualsToken,
    AmpersandEqualsToken,
    PipeEqualsToken,
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
    LetKeyword,
    VarKeyword,
    IfKeyword,
    ElseKeyword,
    WhileKeyword,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
    
    // Nodes
    CompilationUnit,
    ElseClause,

    // Statements
    BlockStatment,
    ExpressionStatement,
    VariableDeclaration,
    AssignOperationStatement,
    IfStatement,
    WhileStatement,
}