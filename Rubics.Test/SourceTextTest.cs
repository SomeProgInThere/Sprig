
using Rubics.Code.Source;

namespace Rubics.Tests;

public class SourceTextTest {

    [Theory]
    [InlineData(".", 1)]
    [InlineData(".\r\n", 2)]
    [InlineData(".\r\n\r\n", 3)]
    public void EvaluateIncludesLastLine(string literal, int lineCount) {
        var sourceText = SourceText.FromString(literal);
        Assert.Equal(lineCount, sourceText.Lines.Length);
    }
}