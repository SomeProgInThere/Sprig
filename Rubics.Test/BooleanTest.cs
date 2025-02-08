
using Rubics.Code;
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class BooleanTest {

    [Theory]
    [MemberData(nameof(BooleanTestExpressions))]
    public void EvaluateBooleanExpressions(TestExpression data) {
        
        var syntaxTree = SyntaxTree.Parse(data.Expression);
        var compilation = new Compilation(syntaxTree);

        // Note: variables are never used
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        
        Assert.Equal(data.ExpectedResult, result.Result);
    }

    public static IEnumerable<object[]> BooleanTestExpressions() {
        return [
            // Basic Operations
            TestExpression.CreateExpression("!true", false),
            TestExpression.CreateExpression("!!true", true),
            TestExpression.CreateExpression("!!!true", false),
            TestExpression.CreateExpression("!!!!true", true),

            // Multiple Operators
            TestExpression.CreateExpression("true == !false", true),
            TestExpression.CreateExpression("!true == false", true),
            TestExpression.CreateExpression("!(true == true)", false),
            TestExpression.CreateExpression("!(false == false)", false),

            // Nested Operations
            TestExpression.CreateExpression("!(!(true == true))", true),
            TestExpression.CreateExpression("!(!true == !false)", true),
            TestExpression.CreateExpression("!(true == !true)", true),
            TestExpression.CreateExpression("!(!true == !!true)", true),

            // Complex Parentheses
            TestExpression.CreateExpression("((!true == false) == !(!true))", true),
            TestExpression.CreateExpression("!(!(true == true) == !(false == false))", false),
            TestExpression.CreateExpression("((!!true == !false) == (true != false))", true),
            TestExpression.CreateExpression("(!(true == false) != !(false == true))", false),
        ];
    }
}