namespace Sprig.Code.Binding;

internal enum BoundNodeKind {
    // Expressions
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    RangeExpression,
    CallExpression,
    CastExpression,
    ErrorExpression,

    // Statements
    BlockStatement,
    VariableDeclaration,
    ExpressionStatement,
    GotoStatement,
    ConditionalGotoStatement,
    LabelStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
}