using System.Collections.Immutable;

using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.IR;

internal sealed class Lowerer() : IR_TreeRewriter {

    public static IR_BlockStatement Lower(FunctionSymbol function, IR_Statement statement) {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
		return FlattenStatements(function, result);
    }

	private static IR_BlockStatement FlattenStatements(FunctionSymbol function, IR_Statement statement) {
		var builder = ImmutableArray.CreateBuilder<IR_Statement>();
		var stack = new Stack<IR_Statement>();
		stack.Push(statement);
		
		while (stack.Count > 0) {
			var current = stack.Pop();
			if (current is IR_BlockStatement block) {
				foreach (var s in block.Statements.Reverse())
					stack.Push(s);
			}
			else {
				builder.Add(current);
			}
		}

		if (function.Type == TypeSymbol.Void) {
			if (builder.Count == 0 || AllowedFallThrough(builder.Last()))
				builder.Add(new IR_ReturnStatment(null));
		}

		return new IR_BlockStatement(builder.ToImmutable());
	}

    private static bool AllowedFallThrough(IR_Statement statement) {
        return statement.Kind != IR_NodeKind.ReturnStatement 
			&& statement.Kind != IR_NodeKind.GotoStatement;
    }

    private LabelSymbol GenerateLabel() {
		var name = $"label{++labelCount}";
		return new LabelSymbol(name);
	}
                                         
    protected override IR_Statement RewriteIfStatement(IR_IfStatement node) {
		if (node.ElseStatement is null) {
			var endLabel = GenerateLabel();

			var gotoCondition = new IR_ConditionalGotoStatement(endLabel, node.Condition, false);
			var endStatement = new IR_LabelStatement(endLabel);
			var result = new IR_BlockStatement([
				gotoCondition, 
				node.IfStatement, 
				endStatement
			]);

			return RewriteStatement(result);
		}
		else {
			var elseLabel = GenerateLabel();
			var endLabel = GenerateLabel();

			var gotoCondition = new IR_ConditionalGotoStatement(elseLabel, node.Condition, false);
			var gotoStatement = new IR_GotoStatement(endLabel);
			var elseStatement = new IR_LabelStatement(elseLabel);
			var endStatement = new IR_LabelStatement(endLabel);
			
			var result = new IR_BlockStatement([
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
	
    protected override IR_Statement RewriteWhileStatement(IR_WhileStatement node) {
		var gotoContinue = new IR_GotoStatement(node.JumpLabel.ContinueLabel);
		var bodyLabel = GenerateLabel();
		var bodyLabelStatement = new IR_LabelStatement(bodyLabel);

		var continueLabelStatement = new IR_LabelStatement(node.JumpLabel.ContinueLabel);
		var gotoCondition = new IR_ConditionalGotoStatement(bodyLabel, node.Condition);
		var breakStatement = new IR_LabelStatement(node.JumpLabel.BrakeLabel);

		var result = new IR_BlockStatement([
			gotoContinue, 
			bodyLabelStatement,
			node.Body, 
			continueLabelStatement, 
			gotoCondition, 
			breakStatement
		]);

		return RewriteStatement(result);
    }

    protected override IR_Statement RewriteDoWhileStatement(IR_DoWhileStatement node) {
        var bodyLabel = GenerateLabel();
		var bodyLabelStatement = new IR_LabelStatement(bodyLabel);

		var continueLabelStatement = new IR_LabelStatement(node.JumpLabel.ContinueLabel);
		var gotoCondition = new IR_ConditionalGotoStatement(bodyLabel, node.Condition);
		var breakStatement = new IR_LabelStatement(node.JumpLabel.BrakeLabel);

		var result = new IR_BlockStatement([
			bodyLabelStatement,
			node.Body, 
			continueLabelStatement,
			gotoCondition, 
			breakStatement
		]);

		return RewriteStatement(result);
    }

    protected override IR_Statement RewriteForStatement(IR_ForStatement node) {
        var range = (IR_RangeExpression)node.Range;
        var variableDeclaration = new IR_VariableDeclaration(node.Variable, range.Lower);
        var variable = new IR_VariableExpression(node.Variable);

		var upperSymbol = new VariableSymbol("upper", true, TypeSymbol.Int32, VariableScope.Local);
		var upperDeclaration = new IR_VariableDeclaration(upperSymbol, range.Upper);

        var condition = new IR_BinaryExpression(
            variable,
            new IR_VariableExpression(upperSymbol),
            IR_BinaryOperator.Bind(SyntaxKind.RightArrowEqualsToken, TypeSymbol.Int32, TypeSymbol.Int32) 
                ?? throw new Exception("Invaild binary operation")
        );

		var continueLabelStatement = new IR_LabelStatement(node.JumpLabel.ContinueLabel);

        var increment = new IR_ExpressionStatement(
            new IR_AssignmentExpression(node.Variable, new IR_BinaryExpression(
                    variable,
                    new IR_LiteralExpression(1),
                    IR_BinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int32, TypeSymbol.Int32)
                        ?? throw new Exception("Invalid binary operation")
                )
            )
        );

        var whileBody = new IR_BlockStatement([
			node.Body, 
			continueLabelStatement, 
			increment
		]);

		var jumpLabel = new JumpLabel(node.JumpLabel.BrakeLabel, GenerateLabel());
        var whileStatement = new IR_WhileStatement(condition, whileBody, jumpLabel);

        var result = new IR_BlockStatement([
			variableDeclaration, 
			upperDeclaration, 
			whileStatement
		]);
 
        return RewriteStatement(result);
    }

	private int labelCount;
}