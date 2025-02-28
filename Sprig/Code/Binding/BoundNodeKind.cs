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
    ErrorExpression,

    // Statements
    BlockStatement,
    ExpressionStatement,
    GotoStatement,
    ConditionalGotoStatement,
    LabelStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    VariableDeclarationStatement,
    AssignOperationStatement,
}