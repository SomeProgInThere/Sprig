
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
            Variable 'x' is already declared
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_UndefinedName() {
        var source = @"
            [x] * 10
        ";

        var diagnostics = @"
            Symbol 'x' is not defined
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
    public void Report_NoErrorForEmptySource() {
        var source = @"[]";
        var diagnostics = @"
            Unexpected token <EndOfFileToken>, expected <IdentifierToken>
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
            Cannot convert type 'Boolean' to 'Int'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);
    }

    [Fact]
    public void Report_UndefinedUnaryOperator() {
        var source = @"
            [+]true
        ";

        var diagnostics = @"
            Unary operator '+' is not defined for type 'Boolean'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);    
    }

    [Fact]
    public void Report_UndefinedBinaryOperator() {
        var source = @"
            10 [*] false
        ";

        var diagnostics = @"
            Binary operator '*' is not defined for types 'Int' and 'Boolean'
        ";

        TestAssert.AssertDiagnostics(source, diagnostics);    
    }
}