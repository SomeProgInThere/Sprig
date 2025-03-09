using System.Collections.Immutable;

using Sprig.Codegen.Binding;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Lowering;

internal sealed class Lowerer() : BoundTreeRewriter {

    public static BoundBlockStatement Lower(BoundStatement statement) {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
		return FlattenStatements(result);
    }

	private static BoundBlockStatement FlattenStatements(BoundStatement statement) {
		var builder = ImmutableArray.CreateBuilder<BoundStatement>();
		var stack = new Stack<BoundStatement>();
		stack.Push(statement);
		
		while (stack.Count > 0) {
			var current = stack.Pop();
			if (current is BoundBlockStatement block) {
				foreach (var s in block.Statements.Reverse())
					stack.Push(s);
			}
			else {
				builder.Add(current);
			}
		}

		return new BoundBlockStatement(builder.ToImmutable());
	}

	private LabelSymbol GenerateLabel() {
		var name = $"label{++labelCount}";
		return new LabelSymbol(name);
	}
                                         
    protected override BoundStatement RewriteIfStatement(BoundIfStatement node) {
		if (node.ElseStatement is null) {
			var endLabel = GenerateLabel();

			var gotoCondition = new BoundConditionalGoto(endLabel, node.Condition, false);
			var endStatement = new BoundLabelStatement(endLabel);
			var result = new BoundBlockStatement([
				gotoCondition, 
				node.IfStatement, 
				endStatement
			]);

			return RewriteStatement(result);
		}
		else {
			var elseLabel = GenerateLabel();
			var endLabel = GenerateLabel();

			var gotoCondition = new BoundConditionalGoto(elseLabel, node.Condition, false);
			var gotoStatement = new BoundGotoStatement(endLabel);
			var elseStatement = new BoundLabelStatement(elseLabel);
			var endStatement = new BoundLabelStatement(endLabel);
			
			var result = new BoundBlockStatement([
				gotoCondition, 
				node.IfStatement, 
				gotoStatement, 
				elseStatement, 
				node.ElseStatement, 
				endStatement
			]);

			return RewriteStatement(result);
		}
    }
	
    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node) {
		var gotoContinue = new BoundGotoStatement(node.JumpLabel.ContinueLabel);
		var bodyLabel = GenerateLabel();
		var bodyLabelStatement = new BoundLabelStatement(bodyLabel);

		var continueLabelStatement = new BoundLabelStatement(node.JumpLabel.ContinueLabel);
		var gotoCondition = new BoundConditionalGoto(bodyLabel, node.Condition);
		var breakStatement = new BoundLabelStatement(node.JumpLabel.BrakeLabel);

		var result = new BoundBlockStatement([
			gotoContinue, 
			bodyLabelStatement,
			node.Body, 
			continueLabelStatement, 
			gotoCondition, 
			breakStatement
		]);

		return RewriteStatement(result);
    }

    protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node) {
        var bodyLabel = GenerateLabel();
		var bodyLabelStatement = new BoundLabelStatement(bodyLabel);

		var continueLabelStatement = new BoundLabelStatement(node.JumpLabel.ContinueLabel);
		var gotoCondition = new BoundConditionalGoto(bodyLabel, node.Condition);
		var breakStatement = new BoundLabelStatement(node.JumpLabel.BrakeLabel);

		var result = new BoundBlockStatement([
			bodyLabelStatement,
			node.Body, 
			continueLabelStatement,
			gotoCondition, 
			breakStatement
		]);

		return RewriteStatement(result);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement node) {
        var range = (BoundRangeExpression)node.Range;
        var variableDeclaration = new BoundVariableDeclaration(node.Variable, range.Lower);
        var variable = new BoundVariableExpression(node.Variable);

		var upperSymbol = new VariableSymbol("upper", true, TypeSymbol.Int, VariableScope.Local);
		var upperDeclaration = new BoundVariableDeclaration(upperSymbol, range.Upper);

        var condition = new BoundBinaryExpression(
            variable,
            new BoundVariableExpression(upperSymbol),
            BinaryOperator.Bind(SyntaxKind.RightArrowEqualsToken, TypeSymbol.Int, TypeSymbol.Int) 
                ?? throw new Exception("Invaild binary operation")
        );

		var continueLabelStatement = new BoundLabelStatement(node.JumpLabel.ContinueLabel);

        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(node.Variable, new BoundBinaryExpression(
                    variable,
                    new BoundLiteralExpression(1),
                    BinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int)
                        ?? throw new Exception("Invalid binary operation")
                )
            )
        );

        var whileBody = new BoundBlockStatement([
			node.Body, 
			continueLabelStatement, 
			increment
		]);

		var jumpLabel = new JumpLabel(node.JumpLabel.BrakeLabel, GenerateLabel());
        var whileStatement = new BoundWhileStatement(condition, whileBody, jumpLabel);

        var result = new BoundBlockStatement([
			variableDeclaration, 
			upperDeclaration, 
			whileStatement
		]);
 
        return RewriteStatement(result);
    }

	private int labelCount;
}