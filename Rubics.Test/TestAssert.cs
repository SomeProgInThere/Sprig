
using Rubics.Code;
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class TestAssert {

    public static void AssertValue(string expression, object value) {
        var syntaxTree = SyntaxTree.Parse(expression);
        var compilation = new Compilation(syntaxTree);

        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        
        Assert.Equal(result.Result, value);
    }

    public static void AssertDiagnostics(string source, string diagnosticSource) {
        var annotatedSource = AnnotatedText.Parse(source);
        
        var syntaxTree = SyntaxTree.Parse(annotatedSource.Source);
        var compilation = new Compilation(syntaxTree);

        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);

        var diagnostics = AnnotatedText.UnindentLines(diagnosticSource);
        if (annotatedSource.Spans.Length != diagnostics.Length)
            throw new Exception("Total spans does not equal expected diagnostics!");

        Assert.Equal(diagnostics.Length, result.Diagnostics.Length);
        for (var i = 0; i < diagnostics.Length; i++) {
            
            var expectedMessage = diagnostics[i];
            var actualMessage = result.Diagnostics[i].Message;
            Assert.Equal(expectedMessage, actualMessage);
        
            var expectedSpan = annotatedSource.Spans[i];
            var actualSpan = result.Diagnostics[i].Span;
            Assert.Equal(expectedSpan, actualSpan);
        }
    }
}