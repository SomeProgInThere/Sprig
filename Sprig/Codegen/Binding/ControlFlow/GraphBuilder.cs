using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Binding.ControlFlow;

internal class GraphBuilder {

    public ControlFlowGraph Build(List<Block> blocks) {
        if (blocks.Count == 0)
            Connect(start, end);
        else
            Connect(start, blocks.First());

        foreach (var block in blocks) {
            foreach (var statement in block.Statements) {

                blockFromStatement.Add(statement, block);
                if (statement is BoundLabelStatement labelStatement)
                    blockFromLabel.Add(labelStatement.Label, block);
            }
        }

        for (int i = 0; i < blocks.Count; i++) {
            var current = blocks[i];
            var next = i == blocks.Count - 1 
                ? end 
                : blocks[i + 1];

            foreach (var statement in current.Statements) {
                var isLast = statement == current.Statements.Last();
                Walk(statement, current, next, isLast);
            }
        }

    Scan:
        foreach (var block in blocks) {
            if (block.Incoming.Count == 0) {
                Remove(blocks, block);
                goto Scan;
            }
        }
        
        blocks.Insert(0, start);
        blocks.Add(end);

        return new ControlFlowGraph(start, end, blocks, branches);
    }

    private void Connect(Block from, Block to, BoundExpression? condition = null) {
        if (condition is BoundLiteralExpression literal) {
            var value = (bool)literal.Value;
            if (value)
                condition = null;
            
            else return;
        }
        
        var branch = new BlockBranch(from, to, condition);
        from.Outgoing.Add(branch);
        to.Incoming.Add(branch);
        
        branches.Add(branch);
    }

    private void Walk(BoundStatement statement, Block current, Block next, bool isLast) {
        switch (statement.Kind) {
            
            case BoundNodeKind.GotoStatement:
                var gotoStatement = (BoundGotoStatement)statement;
                var toBlock = blockFromLabel[gotoStatement.Label];
                Connect(current, toBlock);
                break;
            
            case BoundNodeKind.ConditionalGotoStatement:
                var conditionalGotoStatement = (BoundConditionalGotoStatement)statement;    
                var ifBlock = blockFromLabel[conditionalGotoStatement.Label];
                var elseBlock = next;

                var negateCondition = Negate(conditionalGotoStatement.Condition);
                
                var ifCondition = conditionalGotoStatement.Jump
                    ? conditionalGotoStatement.Condition
                    : negateCondition;
                
                var elseCondition = conditionalGotoStatement.Jump
                    ? negateCondition
                    : conditionalGotoStatement.Condition;

                Connect(current, ifBlock, ifCondition);
                Connect(current, elseBlock, elseCondition);
                break;

            case BoundNodeKind.ReturnStatement:
                Connect(current, end);
                break;

            case BoundNodeKind.VariableDeclaration:
            case BoundNodeKind.LabelStatement:
            case BoundNodeKind.ExpressionStatement:
                if (isLast)
                    Connect(current, next);
                break;

            default:
                throw new Exception($"Unexpected statement: {statement.Kind}");
        }
    }

    private void Remove(List<Block> blocks, Block block) {
        foreach (var branch in block.Incoming) {
            branch.From.Outgoing.Remove(branch);
            branches.Remove(branch);
        }
        
        foreach (var branch in block.Outgoing) {
            branch.To.Incoming.Remove(branch);
            branches.Remove(branch);
        }
        
        blocks.Remove(block);
    }

    private static BoundExpression Negate(BoundExpression condition) {
        if (condition is BoundLiteralExpression literal) {
            var value = (bool)literal.Value;
            return new BoundLiteralExpression(!value);
        }

        var op = BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool);
        return new BoundUnaryExpression(condition, op);
    }

    private readonly Dictionary<BoundStatement, Block> blockFromStatement = [];
    private readonly Dictionary<LabelSymbol, Block> blockFromLabel = [];
    private readonly List<BlockBranch> branches = [];

    private readonly Block start = new(true);
    private readonly Block end = new(false);
}