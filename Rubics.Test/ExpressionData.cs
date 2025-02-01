
namespace Rubics.Tests;

public sealed class ExpressionData(string expression, object expectedResult) {
    public string Expression { get; } = expression;
    public object ExpectedResult { get; } = expectedResult;

    public static object[] CreateExpression(string expression, object expectedResult) {
        return [new ExpressionData(expression, expectedResult)]; 
    }
}
