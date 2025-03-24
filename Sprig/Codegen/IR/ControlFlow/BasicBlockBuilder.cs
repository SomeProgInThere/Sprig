
namespace Sprig.Codegen.IR.ControlFlow;

internal sealed class BasicBlockBuilder {
    
    public List<BasicBlock> Build(IR_BlockStatement block) {
        
        foreach (var statement in block.Statements) {
            switch (statement.Kind) {
                case IR_NodeKind.LabelStatement:
                    StartBlock();
                    statements.Add(statement);
                    break;

                case IR_NodeKind.GotoStatement:
                case IR_NodeKind.ConditionalGotoStatement:
                case IR_NodeKind.ReturnStatement:
                    statements.Add(statement);
                    StartBlock();
                    break;
                
                case IR_NodeKind.NopStatement:
                case IR_NodeKind.VariableDeclaration:
                case IR_NodeKind.ExpressionStatement:
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
    
    private readonly List<IR_Statement> statements = [];
    private readonly List<BasicBlock> blocks = [];
}