namespace Sprig.Code.Syntax;

public enum SyntaxKind {
    // Tokens
    WhitespaceToken,
    EndOfFileToken,
    BadToken,
    IdentifierToken,
    StringToken,
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
    CommaToken,
    ColonToken,

    DoubleAmpersandToken,
    DoublePipeToken,
    DoublePlusToken,
    DoubleMinusToken,
    DoubleStarToken,
    DoubleSlashToken,
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
    DoKeyword,
    ForKeyword,
    InKeyword,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
    RangeExpression,
    CallExpression,
    
    // Other
    CompilationUnit,
    
    // Clause
    TypeClause,
    ElseClause,

    // Statements
    BlockStatment,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
}