
namespace Sprig.Codegen.IRGeneration;

internal enum IRNodeKind {
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

internal abstract class IRNode {
    public abstract IRNodeKind Kind { get; }

    public override string ToString() {
        using var writer = new StringWriter();
        this.WriteTo(writer);
        return writer.ToString();
    }
}