
using System.CodeDom.Compiler;

namespace Sprig.Codegen.IRGeneration.ControlFlow;

internal sealed class BasicBlock {
    public BasicBlock() {}
    
    public BasicBlock(bool isStart) {
        IsStart = isStart;
        IsEnd = !isStart;
    }
    
    public override string ToString() {
        if (IsStart)
            return "<Start>";
    
        if (IsEnd)
            return "<End>";

        using var writer = new StringWriter();
        using var indentedWriter = new IndentedTextWriter(writer);

        foreach (var statement in Statements)
            statement.WriteTo(indentedWriter);

        return writer.ToString();
    }

    public bool IsStart { get; }
    public bool IsEnd { get; }

    public List<IRStatement> Statements { get; } = [];
    public List<BasicBlockBranch> Incoming { get; } = [];
    public List<BasicBlockBranch> Outgoing { get; } = [];
}

internal sealed class BasicBlockBranch(BasicBlock from, BasicBlock to, IRExpression? condition) {

    public override string ToString() {
        if (Condition is null)
            return string.Empty;

        return Condition.ToString();
    }

    public BasicBlock From { get; } = from;
    public BasicBlock To { get; } = to;
    public IRExpression? Condition { get; } = condition;
}