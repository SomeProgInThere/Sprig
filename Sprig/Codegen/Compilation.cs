using System.Collections.Immutable;

using Sprig.Codegen.Binding;
using Sprig.Codegen.Lowering;
using Sprig.Codegen.Syntax;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Binding.ControlFlow;

namespace Sprig.Codegen;

public sealed class Compilation {
    
    public Compilation(params SyntaxTree[] syntaxTrees) 
        : this(null, syntaxTrees) {}

    private Compilation(Compilation? previous, params SyntaxTree[] syntaxTrees) {
        Previous = previous;
        SyntaxTrees = [..syntaxTrees];
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables, bool outputControlFlowGraph = false) {
        var parseDiagnostics = SyntaxTrees.SelectMany(tree => tree.Diagnostics);

        var diagnostics = parseDiagnostics
            .Concat(GlobalScope?.Diagnostics ?? [])
            .ToImmutableArray();
        
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics);

        var program = Binder.BindProgram(GlobalScope);
        if (program.Diagnostics.Any())
            return new EvaluationResult([..program.Diagnostics]);
        
        var statement = GetStatement();

        if (outputControlFlowGraph) {
            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var graphPath = Path.Combine(appDirectory, "cfg.dot");

            var controlFlowStatments = !statement.Statements.Any() 
                && !program.Functions.IsEmpty
                    ? program.Functions.Last().Value
                    : statement;

            var controlFlowGraph = ControlFlowGraph.Create(controlFlowStatments);

            using var writer = new StreamWriter(graphPath);
            controlFlowGraph.WriteTo(writer);
        }

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
                if (!GlobalScope.Symbols.Contains(functionBody.Key))
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
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    internal BoundGlobalScope? GlobalScope {
        get {
            if (globalScope is null) {
                var prevGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref globalScope, prevGlobalScope, null);
            }
            return globalScope;
        }
    }

    private BoundGlobalScope? globalScope;
}