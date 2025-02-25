
namespace Sprig.Tests;

public class InlineTest {

    [Theory]
    [InlineData("{ var x = 0 if x == 0 { x = 10 } else { x = 5 } x }", 10)]
    [InlineData("{ var i = 10 var x = 0 while i > 0 { x += i i -= 1 } x }", 55)]
    [InlineData("{ var x = 0 for i in 0..10 { x += i } x }", 55)]
    [InlineData("{ var x = 10 for i in 1..(x = x - 1) {} x }", 9)]
    public void Evaluate_ComputeCorrectValues(string expression, object value) {
        TestAssert.AssertValue(expression, value);
    }
}