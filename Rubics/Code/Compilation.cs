
using System.Collections.Immutable;
using Rubics.Code.Binding;
using Rubics.Code.Syntax;

namespace Rubics.Code;

public sealed class Compilation {

    public Compilation(SyntaxTree syntaxTree) 
        : this(null, syntaxTree) {}

    private Compilation(Compilation? previous, SyntaxTree syntaxTree) {
        Previous = previous;
        SyntaxTree = syntaxTree;
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables) {
        var diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope?.Diagnostics ?? []).ToImmutableArray();        
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics);
        
        var evaluator = new Evaluator(GlobalScope?.Expression, variables);
        var result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree) => new(this, syntaxTree);

    public Compilation? Previous { get; }
    public SyntaxTree SyntaxTree { get; }

    internal BoundGlobalScope? GlobalScope {
        get {
            if (globalScope == null) {
                var prevGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                Interlocked.CompareExchange(ref globalScope, prevGlobalScope, null);
            }
            return globalScope;
        }
    }

    private BoundGlobalScope? globalScope;
}