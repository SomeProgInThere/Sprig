namespace Sprig.Codegen.Syntax;

public enum SyntaxKind {
    // Trivia
    BadTokenTrivia,
    WhitespaceTrivia,
    SinglelineCommentTrivia,
    MultilineCommentTrivia,

    // Special tokens
    EndOfFileToken,
    IdentifierToken,
    StringToken,
    NumberToken,

    // Simple tokens
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
    OpenParenthesisToken,
    ClosedParenthesisToken,
    OpenBraceToken,
    ClosedBraceToken,

    // Compound Tokens
    DoubleAmpersandToken,
    DoublePipeToken,
    DoubleEqualsToken,
    DoubleLeftArrowToken,
    DoubleRightArrowToken,
    DoubleDotToken,
    BangEqualsToken,
    LeftArrowEqualsToken,
    RightArrowEqualsToken,

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
    FuncKeyword,
    BreakKeyword,
    ContinueKeyword,
    ReturnKeyword,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
    CallExpression,

    // Members
    CompilationUnit,
    GlobalStatement,
    FunctionHeader,
    FunctionParameter,
    
    // Clauses
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
    BreakStatement,
    ContinueStatement,
    ReturnStatement,
}