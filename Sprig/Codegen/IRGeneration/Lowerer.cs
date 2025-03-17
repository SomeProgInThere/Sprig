using System.Collections.Immutable;

using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IRGeneration;

internal sealed class Lowerer() : IRTreeRewriter {

    public static IRBlockStatement Lower(FunctionSymbol function, IRStatement statement) {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
		return FlattenStatements(function, result);
    }

	private static IRBlockStatement FlattenStatements(FunctionSymbol function, IRStatement statement) {
		var builder = ImmutableArray.CreateBuilder<IRStatement>();
		var stack = new Stack<IRStatement>();
		stack.Push(statement);
		
		while (stack.Count > 0) {
			var current = stack.Pop();
			if (current is IRBlockStatement block) {
				foreach (var s in block.Statements.Reverse())
					stack.Push(s);
			}
			else {
				builder.Add(current);
			}
		}

		if (function.Type == TypeSymbol.Void) {
			if (builder.Count == 0 || AllowedFallThrough(builder.Last()))
				builder.Add(new IRReturnStatment(null));
		}

		return new IRBlockStatement(builder.ToImmutable());
	}

    private static bool AllowedFallThrough(IRStatement statement) {
        return statement.Kind != IRNodeKind.ReturnStatement 
			&& statement.Kind != IRNodeKind.GotoStatement;
    }

    private LabelSymbol GenerateLabel() {
		var name = $"label{++labelCount}";
		return new LabelSymbol(name);
	}
                                         
    protected override IRStatement RewriteIfStatement(IRIfStatement node) {
		if (node.ElseStatement is null) {
			var endLabel = GenerateLabel();

			var gotoCondition = new IRConditionalGotoStatement(endLabel, node.Condition, false);
			var endStatement = new IRLabelStatement(endLabel);
			var result = new IRBlockStatement([
				gotoCondition, 
				node.IfStatement, 
				endStatement
			]);

			return RewriteStatement(result);
		}
		else {
			var elseLabel = GenerateLabel();
			var endLabel = GenerateLabel();

			var gotoCondition = new IRConditionalGotoStatement(elseLabel, node.Condition, false);
			var gotoStatement = new IRGotoStatement(endLabel);
			var elseStatement = new IRLabelStatement(elseLabel);
			var endStatement = new IRLabelStatement(endLabel);
			
			var result = new IRBlockStatement([
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
	
    protected override IRStatement RewriteWhileStatement(IRWhileStatement node) {
		var gotoContinue = new IRGotoStatement(node.JumpLabel.ContinueLabel);
		var bodyLabel = GenerateLabel();
		var bodyLabelStatement = new IRLabelStatement(bodyLabel);

		var continueLabelStatement = new IRLabelStatement(node.JumpLabel.ContinueLabel);
		var gotoCondition = new IRConditionalGotoStatement(bodyLabel, node.Condition);
		var breakStatement = new IRLabelStatement(node.JumpLabel.BrakeLabel);

		var result = new IRBlockStatement([
			gotoContinue, 
			bodyLabelStatement,
			node.Body, 
			continueLabelStatement, 
			gotoCondition, 
			breakStatement
		]);

		return RewriteStatement(result);
    }

    protected override IRStatement RewriteDoWhileStatement(IRDoWhileStatement node) {
        var bodyLabel = GenerateLabel();
		var bodyLabelStatement = new IRLabelStatement(bodyLabel);

		var continueLabelStatement = new IRLabelStatement(node.JumpLabel.ContinueLabel);
		var gotoCondition = new IRConditionalGotoStatement(bodyLabel, node.Condition);
		var breakStatement = new IRLabelStatement(node.JumpLabel.BrakeLabel);

		var result = new IRBlockStatement([
			bodyLabelStatement,
			node.Body, 
			continueLabelStatement,
			gotoCondition, 
			breakStatement
		]);

		return RewriteStatement(result);
    }

    protected override IRStatement RewriteForStatement(IRForStatement node) {
        var range = (IRRangeExpression)node.Range;
        var variableDeclaration = new IRVariableDeclaration(node.Variable, range.Lower);
        var variable = new IRVariableExpression(node.Variable);

		var upperSymbol = new VariableSymbol("upper", true, TypeSymbol.Int, VariableScope.Local);
		var upperDeclaration = new IRVariableDeclaration(upperSymbol, range.Upper);

        var condition = new IRBinaryExpression(
            variable,
            new IRVariableExpression(upperSymbol),
            IRBinaryOperator.Bind(SyntaxKind.RightArrowEqualsToken, TypeSymbol.Int, TypeSymbol.Int) 
                ?? throw new Exception("Invaild binary operation")
        );

		var continueLabelStatement = new IRLabelStatement(node.JumpLabel.ContinueLabel);

        var increment = new IRExpressionStatement(
            new IRAssignmentExpression(node.Variable, new IRBinaryExpression(
                    variable,
                    new IRLiteralExpression(1),
                    IRBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int)
                        ?? throw new Exception("Invalid binary operation")
                )
            )
        );

        var whileBody = new IRBlockStatement([
			node.Body, 
			continueLabelStatement, 
			increment
		]);

		var jumpLabel = new JumpLabel(node.JumpLabel.BrakeLabel, GenerateLabel());
        var whileStatement = new IRWhileStatement(condition, whileBody, jumpLabel);

        var result = new IRBlockStatement([
			variableDeclaration, 
			upperDeclaration, 
			whileStatement
		]);
 
        return RewriteStatement(result);
    }

	private int labelCount;
}