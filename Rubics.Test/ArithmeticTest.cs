
using Rubics.Code;
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class ArithmeticTest {

    [Theory]
    [MemberData(nameof(ArithmeticTestData))]
    public void EvaluateArithmeticExpressions(ExpressionData data) {
        
        var syntaxTree = SyntaxTree.Parse(data.Expression);
        var compilation = new Compilation(syntaxTree);

        // Note: variables are never used
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        
        Assert.Equal(data.ExpectedResult, result.Result);
    }

    public static IEnumerable<object[]> ArithmeticTestData() {
        return [
            // Single Operator
            ExpressionData.CreateExpression("1 + 2", 3),
            ExpressionData.CreateExpression("5 - 3", 2),
            ExpressionData.CreateExpression("4 * 3", 12),
            ExpressionData.CreateExpression("8 / 2", 4),
            
            // Multiple Operators
            ExpressionData.CreateExpression("2 + 3 + 4", 9),
            ExpressionData.CreateExpression("10 - 3 - 2", 5),
            ExpressionData.CreateExpression("2 * 3 * 4", 24),
            ExpressionData.CreateExpression("24 / 3 / 2", 4),
            
            // Mixed Operators
            ExpressionData.CreateExpression("2 + 3 * 4", 14),
            ExpressionData.CreateExpression("10 - 6 / 2", 7),
            ExpressionData.CreateExpression("15 + 3 * 2 - 4", 17),
            ExpressionData.CreateExpression("20 / 4 + 3 * 2", 11),
            
            // Simple Parentheses
            ExpressionData.CreateExpression("(2 + 3) * 4", 20),
            ExpressionData.CreateExpression("6 * (8 - 3)", 30),
            ExpressionData.CreateExpression("(15 + 5) / 5", 4),
            ExpressionData.CreateExpression("(12 - 2) * 3", 30),
            
            // Nested Parentheses
            ExpressionData.CreateExpression("2 * (3 + (4 * 2))", 22),
            ExpressionData.CreateExpression("((8 + 4) * 2) - 6", 18),
            ExpressionData.CreateExpression("(20 / (2 + 3)) + 7", 11),
            ExpressionData.CreateExpression("(30 - (5 * 2)) / (5 - 2)", 6),
            
            // Complex Expressions
            ExpressionData.CreateExpression("((15 - 5) * (8 / 2)) + 10", 50),
            ExpressionData.CreateExpression("(25 + (5 * 5)) / (20 - 15)", 10),
            ExpressionData.CreateExpression("((20 + 4) * (6 - 2)) - 15", 81),
            ExpressionData.CreateExpression("(100 / (5 + 5)) * (8 + 2)", 100),
        ];
    }
}