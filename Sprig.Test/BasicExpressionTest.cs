
using Sprig.Code;
using Sprig.Code.Syntax;

namespace Sprig.Tests;

public class BasixExpressionTest {

    [Theory]
    [MemberData(nameof(BasicTestExpressions))]
    public void EvaluateBasicExpressions(TestExpression data) {
        
        var syntaxTree = SyntaxTree.Parse(data.Expression);
        var compilation = new Compilation(syntaxTree);

        // Note: variables are never used
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        
        Assert.Equal(data.ExpectedResult, result.Result);
    }

    public static IEnumerable<object[]> BasicTestExpressions() {
        return [
            // Single Operator
            TestExpression.CreateExpression("1 + 2", 3),
            TestExpression.CreateExpression("5 - 3", 2),
            TestExpression.CreateExpression("4 * 3", 12),
            TestExpression.CreateExpression("8 / 2", 4),
            
            // Parentheses
            TestExpression.CreateExpression("2 * (3 + (4 * 2))", 22),
            TestExpression.CreateExpression("((8 + 4) * 2) - 6", 18),
            TestExpression.CreateExpression("(20 / (2 + 3)) + 7", 11),
            TestExpression.CreateExpression("(30 - (5 * 2)) / (5 - 2)", 6),

            // Comparisons
            TestExpression.CreateExpression("5 > 3", true),
            TestExpression.CreateExpression("3 >= 3", true),
            TestExpression.CreateExpression("2 < 5", true),
            TestExpression.CreateExpression("4 <= 4", true),
            TestExpression.CreateExpression("6 > 8", false),
            TestExpression.CreateExpression("7 <= 5", false),

            // Boolean Logic
            TestExpression.CreateExpression("true && true", true),
            TestExpression.CreateExpression("true || false", true),
            TestExpression.CreateExpression("false && true", false),
            TestExpression.CreateExpression("!true", false),
            TestExpression.CreateExpression("!(5 < 3)", true),

            // Bitwise Operations
            TestExpression.CreateExpression("60 & 13", 12),
            TestExpression.CreateExpression("60 | 13", 61),
            TestExpression.CreateExpression("60 ^ 13", 49),
            TestExpression.CreateExpression("8 << 2", 32), 
            TestExpression.CreateExpression("32 >> 2", 8), 

            // Combined Operations
            TestExpression.CreateExpression("(5 > 3) && (2 < 4)", true),
            TestExpression.CreateExpression("(10 & 6) > (2 | 1)", false),
            TestExpression.CreateExpression("(8 << 1) <= (32 + 16)", true),
            TestExpression.CreateExpression("(15 | 3) >= (12 & 7)", true),
        ];
    }
}