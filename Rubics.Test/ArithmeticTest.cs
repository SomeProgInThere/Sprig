
using Rubics.Code;
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class ArithmeticTest {

    [Theory]
    [MemberData(nameof(ArithmeticTestExpressions))]
    public void EvaluateArithmeticExpressions(TestExpression data) {
        
        var syntaxTree = SyntaxTree.Parse(data.Expression);
        var compilation = new Compilation(syntaxTree);

        // Note: variables are never used
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        
        Assert.Equal(data.ExpectedResult, result.Result);
    }

    public static IEnumerable<object[]> ArithmeticTestExpressions() {
        return [
            // Single Operator
            TestExpression.CreateExpression("1 + 2", 3),
            TestExpression.CreateExpression("5 - 3", 2),
            TestExpression.CreateExpression("4 * 3", 12),
            TestExpression.CreateExpression("8 / 2", 4),
            
            // Multiple Operators
            TestExpression.CreateExpression("2 + 3 + 4", 9),
            TestExpression.CreateExpression("10 - 3 - 2", 5),
            TestExpression.CreateExpression("2 * 3 * 4", 24),
            TestExpression.CreateExpression("24 / 3 / 2", 4),
            
            // Mixed Operators
            TestExpression.CreateExpression("2 + 3 * 4", 14),
            TestExpression.CreateExpression("10 - 6 / 2", 7),
            TestExpression.CreateExpression("15 + 3 * 2 - 4", 17),
            TestExpression.CreateExpression("20 / 4 + 3 * 2", 11),
            
            // Simple Parentheses
            TestExpression.CreateExpression("(2 + 3) * 4", 20),
            TestExpression.CreateExpression("6 * (8 - 3)", 30),
            TestExpression.CreateExpression("(15 + 5) / 5", 4),
            TestExpression.CreateExpression("(12 - 2) * 3", 30),
            
            // Nested Parentheses
            TestExpression.CreateExpression("2 * (3 + (4 * 2))", 22),
            TestExpression.CreateExpression("((8 + 4) * 2) - 6", 18),
            TestExpression.CreateExpression("(20 / (2 + 3)) + 7", 11),
            TestExpression.CreateExpression("(30 - (5 * 2)) / (5 - 2)", 6),
            
            // Complex Expressions
            TestExpression.CreateExpression("((15 - 5) * (8 / 2)) + 10", 50),
            TestExpression.CreateExpression("(25 + (5 * 5)) / (20 - 15)", 10),
            TestExpression.CreateExpression("((20 + 4) * (6 - 2)) - 15", 81),
            TestExpression.CreateExpression("(100 / (5 + 5)) * (8 + 2)", 100),
        ];
    }
}