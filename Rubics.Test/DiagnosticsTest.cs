
using Rubics.Code;
using Rubics.Code.Syntax;

namespace Rubics.Tests;

public class DiagnosticsTest {

    [Fact]
    public void ReportVariableRedeclaration() {
        var source = @" 
        {
            var x = 10
            var y = 20
            {
                var x = 30
            }
            var [x] = 5
        }
        ";

        var diagnostics = @"
            Variable 'x' is already declared
        ";

        AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void ReportUndefinedName() {
        var source = @"
            [x] * 10
        ";

        var diagnostics = @"
            Symbol 'x' is not defined
        ";

        AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void ReportCannotAssign() {
        var source = @"
            {
                let x = 10
                x [=] 20
            }
        ";

        var diagnostics = @"
            Cannot assign value to immutabe variable 'x'
        ";

        AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void ReportCannotConvert() {
        var source = @"
            {
                var x = 10
                x = [true]
            }
        ";

        var diagnostics = @"
            Cannot convert type 'System.Boolean' to 'System.Int32'
        ";

        AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void ReportUndefinedUnaryOperator() {
        var source = @"
            [+]true
        ";

        var diagnostics = @"
            Unary operator '+' is not defined for type 'System.Boolean'
        ";

        AssertDiagnostics(source, diagnostics);    
    }

    [Fact]
    public void ReportUndefinedBinaryOperator() {
        var source = @"
            10 [*] false
        ";

        var diagnostics = @"
            Binary operator '*' is not defined for types 'System.Int32' and 'System.Boolean'
        ";

        AssertDiagnostics(source, diagnostics);    
    }

    private static void AssertDiagnostics(string source, string diagnosticSource) {
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