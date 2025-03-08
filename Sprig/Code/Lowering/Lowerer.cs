using System.Collections.Immutable;

using Sprig.Code.Binding;
using Sprig.Code.Symbols;
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
		var name = $"l{++labelCount}";
		return new LabelSymbol(name);
	}

    /*
		Iif statement						|	if-else statement
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

			var gotoCondition = new BoundConditionalGoto(endLabel, node.Condition, false);
			var endStatement = new BoundLabelStatement(endLabel);
			var result = new BoundBlockStatement([gotoCondition, node.IfStatement, endStatement]);

			return RewriteStatement(result);
		}
		else {
			var elseLabel = GenerateLabel();
			var endLabel = GenerateLabel();

			var gotoCondition = new BoundConditionalGoto(elseLabel, node.Condition, false);
			var gotoStatement = new BoundGotoStatement(endLabel);
			var elseStatement = new BoundLabelStatement(elseLabel);
			var endStatement = new BoundLabelStatement(endLabel);
			var result = new BoundBlockStatement(
				[gotoCondition, node.IfStatement, gotoStatement, elseStatement, node.ElseStatement, endStatement]
			);

			return RewriteStatement(result);
		}
    }
	
	/*
		while-statement

		HL-IR:
			while <condition>
				<body>
		
		LL-IR:
			goto check
			continue:
				<body>
			check:
				goto <condition> continue
	*/
    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node) {
		var continueLabel = GenerateLabel();
		var checkLabel = GenerateLabel();

		var gotoCheck = new BoundGotoStatement(checkLabel);
		
		var continueStatement = new BoundLabelStatement(continueLabel);
		var checkStatement = new BoundLabelStatement(checkLabel);
		var gotoCondition = new BoundConditionalGoto(continueLabel, node.Condition);

		var result = new BoundBlockStatement(
			[gotoCheck, continueStatement, node.Body, checkStatement, gotoCondition]
		);

		return RewriteStatement(result);
    }

	/*
		do-while-statement

		HL-IR:
			do
				<body>
			while <condition>
		
		LL-IR:
			continue:
				<body>
			goto <condition> continue
	*/
    protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node) {
		var continueLabel = GenerateLabel();

		var continueStatement = new BoundLabelStatement(continueLabel);
		var gotoCondition = new BoundConditionalGoto(continueLabel, node.Condition);
		
		var result = new BoundBlockStatement(
			[continueStatement, node.Body, gotoCondition]
		);

		return RewriteStatement(result);
    }

    /*  
		for-statement

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

        var step = new BoundExpressionStatement(
            new BoundAssignmentExpression(node.Variable, new BoundBinaryExpression(
                    variable,
                    new BoundLiteralExpression(1),
                    BinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int)
                        ?? throw new Exception("Invalid binary operation")
                )
            )
        );

        var whileBody = new BoundBlockStatement([node.Body, step]);
        var whileStatement = new BoundWhileStatement(condition, whileBody);
        var result = new BoundBlockStatement([variableDeclaration, upperDeclaration, whileStatement]);

        return RewriteStatement(result);
    }

	private int labelCount;
}