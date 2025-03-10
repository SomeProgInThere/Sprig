using System.Collections.Immutable;

using Sprig.Codegen.Binding;
using Sprig.Codegen.Lowering;
using Sprig.Codegen.Syntax;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

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

        var program = Binder.BindProgram(GlobalScope);
        if (program.Diagnostics.Any())
            return new EvaluationResult([..program.Diagnostics]);
        
        var statement = GetStatement();
        var evaluator = new Evaluator(program.Functions, statement, variables);
        var result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree) => new(this, syntaxTree);

    public void EmitTree(TextWriter writer) {
        var statement = GetStatement();
        
        if (statement.Statements.Any())
            statement.WriteTo(writer);

        else {
            var program = Binder.BindProgram(GlobalScope);
            foreach (var functionBody in program.Functions) {
                if (!GlobalScope.Functions.Contains(functionBody.Key))
                    continue;
                
                functionBody.Key.WriteTo(writer);
                functionBody.Value.WriteTo(writer);
            }
        }
    }

    private BoundBlockStatement GetStatement() {
        if (GlobalScope is null)
            throw new Exception("GlobalScope is null");

        var result = GlobalScope.Statement;
        return Lowerer.Lower(result);
    }

    public Compilation? Previous { get; }
    public SyntaxTree SyntaxTree { get; }

    internal BoundGlobalScope? GlobalScope {
        get {
            if (globalScope is null) {
                var prevGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                Interlocked.CompareExchange(ref globalScope, prevGlobalScope, null);
            }
            return globalScope;
        }
    }

    private BoundGlobalScope? globalScope;
}