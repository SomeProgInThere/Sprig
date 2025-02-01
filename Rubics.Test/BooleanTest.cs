
using Rubics.Code;
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class BooleanTest {

    [Theory]
    [MemberData(nameof(BooleanTestData))]
    public void EvaluateBooleanExpressions(ExpressionData data) {
        
        var syntaxTree = SyntaxTree.Parse(data.Expression);
        var compilation = new Compilation(syntaxTree);

        // Note: variables are never used
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        
        Assert.Equal(data.ExpectedResult, result.Result);
    }

    public static IEnumerable<object[]> BooleanTestData() {
        return [
            // Basic Operations
            ExpressionData.CreateExpression("!true", false),
            ExpressionData.CreateExpression("!!true", true),
            ExpressionData.CreateExpression("!!!true", false),
            ExpressionData.CreateExpression("!!!!true", true),

            // Multiple Operators
            ExpressionData.CreateExpression("true == !false", true),
            ExpressionData.CreateExpression("!true == false", true),
            ExpressionData.CreateExpression("!(true == true)", false),
            ExpressionData.CreateExpression("!(false == false)", false),

            // Nested Operations
            ExpressionData.CreateExpression("!(!(true == true))", true),
            ExpressionData.CreateExpression("!(!true == !false)", true),
            ExpressionData.CreateExpression("!(true == !true)", true),
            ExpressionData.CreateExpression("!(!true == !!true)", true),

            // Complex Parentheses
            ExpressionData.CreateExpression("((!true == false) == !(!true))", true),
            ExpressionData.CreateExpression("!(!(true == true) == !(false == false))", false),
            ExpressionData.CreateExpression("((!!true == !false) == (true != false))", true),
            ExpressionData.CreateExpression("(!(true == false) != !(false == true))", false),
        ];
    }
}