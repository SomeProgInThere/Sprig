namespace Sprig.Codegen.Binding;

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
    LabelStatement,
    GotoStatement,
    ConditionalGotoStatement,
    ReturnStatement,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
}