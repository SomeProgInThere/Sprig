
namespace Rubics.Code.Binding;

internal enum BoundKind {
    // Statements
    BlockStatement,
    ExpressionStatement,
    
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
