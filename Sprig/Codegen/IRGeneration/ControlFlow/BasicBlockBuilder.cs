
namespace Sprig.Codegen.IRGeneration.ControlFlow;

internal sealed class BasicBlockBuilder {
    
    public List<BasicBlock> Build(IRBlockStatement block) {
        
        foreach (var statement in block.Statements) {
            switch (statement.Kind) {
                case IRNodeKind.LabelStatement:
                    StartBlock();
                    statements.Add(statement);
                    break;

                case IRNodeKind.GotoStatement:
                case IRNodeKind.ConditionalGotoStatement:
                case IRNodeKind.ReturnStatement:
                    statements.Add(statement);
                    StartBlock();
                    break;
                
                case IRNodeKind.VariableDeclaration:
                case IRNodeKind.ExpressionStatement:
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
            var block = new BasicBlock();
            block.Statements.AddRange(statements);
            blocks.Add(block);
            statements.Clear();
        }
    }
    
    private readonly List<IRStatement> statements = [];
    private readonly List<BasicBlock> blocks = [];
}