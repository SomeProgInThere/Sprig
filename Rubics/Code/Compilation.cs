
using Rubics.Code.Binding;
using Rubics.Code.Syntax;

namespace Rubics.Code;

public sealed class Compilation(SyntaxTree syntax) {

    public EvaluationResult Evaluate(Dictionary<string, object> variables) {
        var binder = new Binder(variables);
        var expression = binder.BindExpression(Syntax.Root);

        var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics);
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics);
        
        var evaluator = new Evaluator(expression, variables);
        var result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public SyntaxTree Syntax { get; } = syntax;
}

public sealed class EvaluationResult(IEnumerable<DiagnosticMessage> diagnostics, object? result = null) {
    public IEnumerable<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public object? Result { get; } = result;
}