
namespace Rubics.Code.Binding;

internal enum BoundKind {
    // Statements
    BlockStatement,
    ExpressionStatement,
    IfStatement,
    WhileStatement,
    VariableDeclaration,
    AssignOperationStatement,
    
    // Expressions
    LiteralExpression,
    UnaryExpression,
    VariableExpression,
    BinaryExpression,
    AssignmentExpression,

}

internal abstract class BoundNode {
    public abstract BoundKind Kind { get; }
}
