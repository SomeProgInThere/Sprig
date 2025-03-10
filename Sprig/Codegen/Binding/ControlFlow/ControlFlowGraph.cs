
namespace Sprig.Codegen.Binding.ControlFlow;

internal class ControlFlowGraph {

    public static ControlFlowGraph Create(BoundBlockStatement body) {
        var blockBuilder = new BlockBuilder();
        var blocks = blockBuilder.Build(body);

        var graphBuilder = new GraphBuilder();
        return graphBuilder.Build(blocks);
    }

    public static bool AllPathsReturn(BoundBlockStatement body) {
        var graph = Create(body);
        
        foreach (var branch in graph.End.Incoming) {
            var lastStatement = branch.From.Statements.Last();
            if (lastStatement.Kind != BoundNodeKind.ReturnStatement)
                return false;
        }

        return true;
    }

    public void WriteTo(TextWriter writer) {

        static string Quote(string text) => "\"" + text.Replace("\"", "\\\"") + "\"";

        writer.WriteLine("digraph G {");
        
        var blockIds = new Dictionary<Block, string>();
        for (var i = 0; i < Blocks.Count; i++) {
            var id = $"N{i}";
            blockIds.Add(Blocks[i], id);
        }

        foreach (var block in Blocks) {
            var id = blockIds[block];
            var label = Quote(block.ToString().Replace(Environment.NewLine, "\\l"));
            
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

    internal ControlFlowGraph(Block start, Block end, List<Block> blocks, List<BlockBranch> branches) {
        Start = start;
        End = end;
        Blocks = blocks;
        Branches = branches;
    }

    public Block Start { get; }
    public Block End { get; }
    public List<Block> Blocks { get; }
    public List<BlockBranch> Branches { get; }
}