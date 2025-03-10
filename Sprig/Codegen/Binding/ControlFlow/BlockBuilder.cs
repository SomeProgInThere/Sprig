
namespace Sprig.Codegen.Binding.ControlFlow;

internal sealed class BlockBuilder {
    
    public List<Block> Build(BoundBlockStatement block) {
        
        foreach (var statement in block.Statements) {
            switch (statement.Kind) {
                case BoundNodeKind.LabelStatement:
                    StartBlock();
                    statements.Add(statement);
                    break;

                case BoundNodeKind.GotoStatement:
                case BoundNodeKind.ConditionalGotoStatement:
                case BoundNodeKind.ReturnStatement:
                    statements.Add(statement);
                    StartBlock();
                    break;
                
                case BoundNodeKind.VariableDeclaration:
                case BoundNodeKind.ExpressionStatement:
                    statements.Add(statement);
                    break;

                default:
                    throw new Exception($"Unexpected statement: {statement.Kind}");
            }
        }

        EndBlock();
        return blocks;
    }

    private void StartBlock() {
        EndBlock();
    }

    private void EndBlock() {
        if (statements.Count > 0) {
            var block = new Block();
            block.Statements.AddRange(statements);
            blocks.Add(block);
            statements.Clear();
        }
    }
    
    private readonly List<BoundStatement> statements = [];
    private readonly List<Block> blocks = [];
}