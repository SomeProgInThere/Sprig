
namespace Sprig.Codegen.Binding.ControlFlow;

internal sealed class Block {
    public Block() {}
    
    public Block(bool isStart) {
        IsStart = isStart;
        IsEnd = !isStart;
    }
    
    public override string ToString() {
        if (IsStart)
            return "<Start>";
    
        if (IsEnd)
            return "<End>";

        using var writer = new StringWriter();
        foreach (var statement in Statements)
            statement.WriteTo(writer);

        return writer.ToString();
    }

    public bool IsStart { get; }
    public bool IsEnd { get; }

    public List<BoundStatement> Statements { get; } = [];
    public List<BlockBranch> Incoming { get; } = [];
    public List<BlockBranch> Outgoing { get; } = [];
}

internal sealed class BlockBranch(Block from, Block to, BoundExpression? condition) {

    public override string ToString() {
        if (Condition is null)
            return string.Empty;

        return Condition.ToString();
    }

    public Block From { get; } = from;
    public Block To { get; } = to;
    public BoundExpression? Condition { get; } = condition;
}