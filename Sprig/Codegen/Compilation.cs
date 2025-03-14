using System.Collections.Immutable;

using Sprig.Codegen.IRGeneration;
using Sprig.Codegen.IRGeneration.ControlFlow;
using Sprig.Codegen.Syntax;
using Sprig.Codegen.Symbols;

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
        
        var program = GetProgram();

        if (outputControlFlowGraph) {
            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var graphPath = Path.Combine(appDirectory, "cfg.dot");

            var controlFlowStatments = !program.Statement.Statements.Any() 
                && !program.Functions.IsEmpty
                    ? program.Functions.Last().Value
                    : program.Statement;

            var controlFlowGraph = ControlFlowGraph.Create(controlFlowStatments);

            using var writer = new StreamWriter(graphPath);
            controlFlowGraph.WriteTo(writer);
        }

        if (program.Diagnostics.Any())
            return new EvaluationResult([..program.Diagnostics]);

        var evaluator = new Evaluator(program, variables);
        var result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree) => new(this, syntaxTree);

    public void EmitTree(TextWriter writer) {
        var program = GetProgram();

        if (program.Statement.Statements.Any())
            program.Statement.WriteTo(writer);

        else {
            foreach (var functionBody in program.Functions) {
                if (!GlobalScope.Symbols.Contains(functionBody.Key))
                    continue;
                
                functionBody.Key.WriteTo(writer);
                writer.WriteLine();
                functionBody.Value.WriteTo(writer);
            }
        }
    }

    private IRProgram GetProgram() {
        var previous = Previous?.GetProgram();
        return Binder.BindProgram(previous, GlobalScope);
    }

    public Compilation? Previous { get; }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    internal GlobalScope? GlobalScope {
        get {
            if (globalScope is null) {
                var prevGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref globalScope, prevGlobalScope, null);
            }
            return globalScope;
        }
    }

    private GlobalScope? globalScope;
}