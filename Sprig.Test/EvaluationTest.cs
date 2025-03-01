
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
            Cannot convert type 'int' to 'bool'
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
            Cannot convert type 'int' to 'bool'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Evaluate_DoWhileStatement_ReportCannotConvert() {
        var source = @" 
            {
                var x = 0
                do {
                    x = 10
                }
                while [10]
            }
        ";

        var diagnostics = @"
            Cannot convert type 'int' to 'bool'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Evaluate_ForStatement_ReportCannotConvert() {
        var source = @" 
            {
                var x = 0
                for i in [false]..10
                    x = x + i
            }
        ";

        var diagnostics = @"
            Range expression initialized with non-integer type
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }
}