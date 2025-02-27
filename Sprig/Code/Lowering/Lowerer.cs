using System.Collections.Immutable;

using Sprig.Code.Binding;
using Sprig.Code.Syntax;

namespace Sprig.Code.Lowering;

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
		var name = $"Label-{++labelCount}";
		return new LabelSymbol(name);
	}

    /*
		If statement						|	If-Else statement
											|
        HL-IR:                          	|   HL-IR:
            if <condition>              	|       if <condition>        
                <then-body>             	|           <then-body>
                                        	|       else
        LL-IR:                          	|           <else-body>
            goto <condition> end      		|   
                <then-body>             	|   LL-IR:
            end:                        	|       goto <condition> else      
                                        	|           <then-body>
                                        	|       goto end
                                        	|       else:
                                        	|            <else-body>
                                        	|       end:
    */                                          
    protected override BoundStatement RewriteIfStatement(BoundIfStatement node) {
		if (node.ElseStatement is null) {
			var endLabel = GenerateLabel();

			var gotoCondition = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
			var endStatement = new BoundLableStatement(endLabel);
			var result = new BoundBlockStatement([gotoCondition, node.IfStatement, endStatement]);

			return RewriteStatement(result);
		}
		else {
			var elseLabel = GenerateLabel();
			var endLabel = GenerateLabel();

			var gotoCondition = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
			var gotoStatement = new BoundGotoStatement(endLabel);
			var elseStatement = new BoundLableStatement(elseLabel);
			var endStatement = new BoundLableStatement(endLabel);
			var result = new BoundBlockStatement(
				[gotoCondition, node.IfStatement, gotoStatement, elseStatement, node.ElseStatement, endStatement]
			);

			return RewriteStatement(result);
		}
    }
	
	/*
		While-statement

		HL-IR:
			while <condition>
				<body>
		
		LL-IR:
			goto check
			continue:
				<body>
			check:
				goto <condition> continue
			end:
	*/
    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node) {
		var continueLabel = GenerateLabel();
		var checkLabel = GenerateLabel();
		var endLabel = GenerateLabel();

		var gotoCheck = new BoundGotoStatement(checkLabel);
		
		var continueStatement = new BoundLableStatement(continueLabel);
		var checkStatement = new BoundLableStatement(checkLabel);
		var gotoCondition = new BoundConditionalGotoStatement(continueLabel, node.Condition);
		var endStatement = new BoundLableStatement(endLabel);

		var result = new BoundBlockStatement(
			[gotoCheck, continueStatement, node.Body, checkStatement, gotoCondition, endStatement]
		);

		return RewriteStatement(result);
    }

    /*  
		For-statement

        HL-IR:
            for <var> in <lower>..<upper>
                <body>
        
        IL-IR:
            var <var> = <lower>
			var <upperBound> = <upper>
            while (<var> <= <upperBound>)
                <body>
                <var> = <var> + 1
    */
    protected override BoundStatement RewriteForStatement(BoundForStatement node) {
        var range = (BoundRangeExpression)node.Range;
        var variableDeclaration = new BoundVariableDeclarationStatement(node.Variable, range.Lower);
        var variable = new BoundVariableExpression(node.Variable);

		var upperBoundSymbol = new VariableSymbol("upperBound", true, typeof(int));
		var upperBoundDeclaration = new BoundVariableDeclarationStatement(upperBoundSymbol, range.Upper);

        var condition = new BoundBinaryExpression(
            variable,
            new BoundVariableExpression(upperBoundSymbol),
            BinaryOperator.Bind(SyntaxKind.RightArrowEqualsToken, typeof(int), typeof(int)) 
                ?? throw new Exception("Invaild binary operation")
        );

        var step = new BoundExpressionStatement(
            new BoundAssignmentExpression(node.Variable, new BoundBinaryExpression(
                    variable,
                    new BoundLiteralExpression(1),
                    BinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int))
                        ?? throw new Exception("Invalid binary operation")
                )
            )
        );

        var whileBody = new BoundBlockStatement([node.Body, step]);
        var whileStatement = new BoundWhileStatement(condition, whileBody);
        var result = new BoundBlockStatement([variableDeclaration, upperBoundDeclaration, whileStatement]);

        return RewriteStatement(result);
    }

	private int labelCount;
}