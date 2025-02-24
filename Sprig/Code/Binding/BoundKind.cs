namespace Sprig.Code.Binding;

internal enum BoundKind {
    // Statements
    BlockStatement,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    VariableDeclarationStatement,
    AssignOperationStatement,
    
    // Expressions
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    RangeExpression,
}