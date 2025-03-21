
namespace Sprig.Codegen.IR;

internal enum IR_NodeKind {
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

internal abstract class IR_Node {
    public abstract IR_NodeKind Kind { get; }

    public override string ToString() {
        using var writer = new StringWriter();
        this.WriteTo(writer);
        return writer.ToString();
    }
}