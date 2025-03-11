
namespace Sprig.Tests;

public class DiagnosticsTest {

    [Fact]
    public void Report_VariableRedeclaration() {
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
            Variable 'x' is already declared in scope
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_UndefinedIdentifier() {
        var source = @"
            [x] * 10
        ";

        var diagnostics = @"
            Variable 'x' does not exist
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_UnterminatedString() {
        var source = @"
            [""]test_case
        ";

        var diagnostics = @"
            Unterminated string literal
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_CannotAssign() {
        var source = @"
            {
                let x = 10
                x [=] 20
            }
        ";

        var diagnostics = @"
            Cannot assign value to immutabe variable 'x'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_CannotConvert() {
        var source = @"
            {
                var x = 10
                x = [true]
            }
        ";

        var diagnostics = @"
            Cannot convert type 'bool' to 'int'. An explicit cast exists (are you missing a cast?)
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_UndefinedUnaryOperator() {
        var source = @"
            [+]true
        ";

        var diagnostics = @"
            Unary operator '+' is not defined for type 'bool'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);    
    }

    [Fact]
    public void Report_UndefinedBinaryOperator() {
        var source = @"
            10 [*] false
        ";

        var diagnostics = @"
            Binary operator '*' is not defined for types 'int' and 'bool'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);    
    }
}