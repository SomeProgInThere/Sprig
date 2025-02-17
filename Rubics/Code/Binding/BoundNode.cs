
namespace Rubics.Code.Binding;

internal enum BoundKind {
    // Statements
    BlockStatement,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    VariableDeclaration,
    AssignOperationStatement,
    
    // Expressions
    LiteralExpression,
    UnaryExpression,
    VariableExpression,
    BinaryExpression,
    AssignmentExpression,
    RangeExpression,
}

internal abstract class BoundNode {
    public abstract BoundKind Kind { get; }
}
