
using System.Collections.Immutable;
using Rubics.Code.Binding;
using Rubics.Code.Syntax;

namespace Rubics.Code;

public sealed class Compilation(SyntaxTree syntax) {

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables) {
        var binder = new Binder(variables);
        var expression = binder.BindExpression(Syntax.Root);

        var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToImmutableArray();
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics);
        
        var evaluator = new Evaluator(expression, variables);
        var result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public SyntaxTree Syntax { get; } = syntax;
}