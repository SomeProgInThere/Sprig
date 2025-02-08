
namespace Rubics.Tests;

public sealed class TestExpression(string expression, object expectedResult) {
    public string Expression { get; } = expression;
    public object ExpectedResult { get; } = expectedResult;

    public static object[] CreateExpression(string expression, object expectedResult) {
        return [new TestExpression(expression, expectedResult)]; 
    }
}
