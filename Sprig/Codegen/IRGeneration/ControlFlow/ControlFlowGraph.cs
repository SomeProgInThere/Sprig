
namespace Sprig.Codegen.IRGeneration.ControlFlow;

internal class ControlFlowGraph {

    public static ControlFlowGraph Create(IRBlockStatement body) {
        var blockBuilder = new BasicBlockBuilder();
        var blocks = blockBuilder.Build(body);

        var graphBuilder = new GraphBuilder();
        return graphBuilder.Build(blocks);
    }

    public static bool AllPathsReturn(IRBlockStatement body) {
        var graph = Create(body);
        
        foreach (var branch in graph.End.Incoming) {
            var lastStatement = branch.From.Statements.LastOrDefault();
            if (lastStatement == null || lastStatement.Kind != IRNodeKind.ReturnStatement)
                return false;
        }

        return true;
    }

    public void WriteTo(TextWriter writer) {

        static string Quote(string text) 
            => "\"" + text.TrimEnd().Replace("\"", "\\\"").Replace(Environment.NewLine, "\\l") + "\""; 

        writer.WriteLine("digraph G {");
        
        var blockIds = new Dictionary<BasicBlock, string>();
        for (var i = 0; i < Blocks.Count; i++) {
            var id = $"N{i}";
            blockIds.Add(Blocks[i], id);
        }

        foreach (var block in Blocks) {
            var id = blockIds[block];
            var label = Quote(block.ToString());
            writer.WriteLine($"\t{id} [label = {label}, shape = box]");
        }

        foreach (var branch in Branches) {
            var fromId = blockIds[branch.From];
            var toId = blockIds[branch.To];
            var label = Quote(branch.ToString());

            writer.WriteLine($"\t{fromId} -> {toId} [label = {label}]");
        }

        writer.WriteLine("}");
    }

    internal ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches) {
        Start = start;
        End = end;
        Blocks = blocks;
        Branches = branches;
    }

    public BasicBlock Start { get; }
    public BasicBlock End { get; }
    public List<BasicBlock> Blocks { get; }
    public List<BasicBlockBranch> Branches { get; }
}