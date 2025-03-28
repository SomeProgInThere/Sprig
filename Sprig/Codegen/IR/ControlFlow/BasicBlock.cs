
using System.CodeDom.Compiler;
using Sprig.IO;

namespace Sprig.Codegen.IR.ControlFlow;

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

    public List<IR_Statement> Statements { get; } = [];
    public List<BasicBlockBranch> Incoming { get; } = [];
    public List<BasicBlockBranch> Outgoing { get; } = [];
}

internal sealed class BasicBlockBranch(BasicBlock from, BasicBlock to, IR_Expression? condition) {

    public override string ToString() {
        if (Condition is null)
            return string.Empty;

        return Condition.ToString();
    }

    public BasicBlock From { get; } = from;
    public BasicBlock To { get; } = to;
    public IR_Expression? Condition { get; } = condition;
}