using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IR.ControlFlow;

internal class GraphBuilder {

    public ControlFlowGraph Build(List<BasicBlock> blocks) {
        if (blocks.Count == 0)
            Connect(start, end);
        else
            Connect(start, blocks.First());

        foreach (var block in blocks) {
            foreach (var statement in block.Statements) {

                blockFromStatement.Add(statement, block);
                if (statement is IR_LabelStatement labelStatement)
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

    private void Connect(BasicBlock from, BasicBlock to, IR_Expression? condition = null) {
        if (condition is IR_LiteralExpression literal) {
            var value = (bool)literal.Value;
            if (value)
                condition = null;
            
            else return;
        }
        
        var branch = new BasicBlockBranch(from, to, condition);
        from.Outgoing.Add(branch);
        to.Incoming.Add(branch);
        
        branches.Add(branch);
    }

    private void Walk(IR_Statement statement, BasicBlock current, BasicBlock next, bool isLast) {
        switch (statement.Kind) {
            
            case IR_NodeKind.GotoStatement:
                var gotoStatement = (IR_GotoStatement)statement;
                var toBlock = blockFromLabel[gotoStatement.Label];
                Connect(current, toBlock);
                break;
            
            case IR_NodeKind.ConditionalGotoStatement:
                var conditionalGotoStatement = (IR_ConditionalGotoStatement)statement;    
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

            case IR_NodeKind.ReturnStatement:
                Connect(current, end);
                break;

            case IR_NodeKind.VariableDeclaration:
            case IR_NodeKind.LabelStatement:
            case IR_NodeKind.ExpressionStatement:
                if (isLast)
                    Connect(current, next);
                break;

            default:
                throw new Exception($"Unexpected statement: {statement.Kind}");
        }
    }

    private void Remove(List<BasicBlock> blocks, BasicBlock block) {
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

    private static IR_Expression Negate(IR_Expression condition) {
        if (condition is IR_LiteralExpression literal) {
            var value = (bool)literal.Value;
            return new IR_LiteralExpression(!value);
        }

        var op = UnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Boolean);
        return new IR_UnaryExpression(condition, op);
    }

    private readonly Dictionary<IR_Statement, BasicBlock> blockFromStatement = [];
    private readonly Dictionary<LabelSymbol, BasicBlock> blockFromLabel = [];
    private readonly List<BasicBlockBranch> branches = [];

    private readonly BasicBlock start = new(true);
    private readonly BasicBlock end = new(false);
}