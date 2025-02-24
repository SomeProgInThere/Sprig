
namespace Sprig.Tests;

public class EvaluationTest {
    
    [Fact]
    public void Evaluate_BlockStatement_NoInfiniteLoop() {
        var source = @" 
            {
            [)][]
        ";

        var diagnostics = @"
            Unexpected token ')', expected <IdentifierToken>
            Unexpected token <EndOfFileToken>, expected '}'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Evaluate_IfStatement_ReportCannotConvert() {
        var source = @" 
            {
                var x = 0
                if [10]
                    x = 10
            }
        ";

        var diagnostics = @"
            Cannot convert type 'System.Int32' to 'System.Boolean'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Evaluate_WhileStatement_ReportCannotConvert() {
        var source = @" 
            {
                var x = 0
                while [10]
                    x = 10
            }
        ";

        var diagnostics = @"
            Cannot convert type 'System.Int32' to 'System.Boolean'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Evaluate_ForStatement_ReportCannotConvert() {
        var source = @" 
            {
                var x = 0
                for i in false..[10]
                    x += i
            }
        ";

        var diagnostics = @"
            Cannot convert type 'System.Int32' to 'System.Boolean'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }
}