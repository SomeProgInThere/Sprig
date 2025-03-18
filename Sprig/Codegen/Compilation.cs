using System.Collections.Immutable;

using Sprig.Codegen.IR_Generation;
using Sprig.Codegen.Syntax;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

public sealed class Compilation {
    
    public static Compilation Create(params SyntaxTree[] syntaxTrees) {
        return new Compilation(previous: null, syntaxTrees);
    }

    public ImmutableArray<DiagnosticMessage> Emit(string moduleName, string[] references, string outputPath) {
        var program = GetProgram();
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        var emitter = new Emitter(program);
        emitter.LoadReferences(moduleName, references);
        emitter.Emit(outputPath);

        return [..emitter.Diagonostics];
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree) => new(this, syntaxTree);

    public void EmitTree(TextWriter writer) {
        var mainFunction = GlobalScope.MainFunction;
        if (mainFunction != null) {

            var program = GetProgram();
            mainFunction.WriteTo(writer);
            writer.WriteLine();

            if (!program.Functions.TryGetValue(mainFunction, out var body))
                return;

            body.WriteTo(writer);
        }
    }

    private Compilation(Compilation? previous, params SyntaxTree[] syntaxTrees) {
        Previous = previous;
        SyntaxTrees = [..syntaxTrees];
    }

    private IR_Program GetProgram() {
        var previous = Previous?.GetProgram();
        return Binder.BindProgram(previous, GlobalScope);
    }

    internal GlobalScope? GlobalScope {
        get {
            if (globalScope is null) {
                var prevGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref globalScope, prevGlobalScope, null);
            }
            return globalScope;
        }
    }

    public Compilation? Previous { get; }
    public FunctionSymbol MainFunction => GlobalScope.MainFunction;
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    private GlobalScope? globalScope;
}