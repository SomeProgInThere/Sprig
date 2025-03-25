using System.Collections.Immutable;

using Sprig.Codegen.IR;
using Sprig.Codegen.Syntax;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.IR.ControlFlow;

namespace Sprig.Codegen;

public sealed class Compilation {
    
    public static Compilation Create(params SyntaxTree[] syntaxTrees) {
        return new Compilation(previous: null, syntaxTrees);
    }

    public ImmutableArray<DiagnosticMessage> Emit(string moduleName, string[] references, string outputPath, string[]? dumpOptions) {
        var program = GetProgram();
        if (program.Diagnostics.Any())
            return program.Diagnostics;

        if (dumpOptions.Length != 0) {
            foreach (var option in dumpOptions) {

                if (option == "lowered")
                    DumpLowered(outputPath, program);

                if (option == "controlflow")
                    DumpControlflow(outputPath, program);

                if (option == "symbols")
                    DumpSymbols(program);
            }
        } 
        
        else {
            var emitter = new Emitter(program);
            emitter.LoadReferences(moduleName, references);
            emitter.Emit(outputPath);

            return [..emitter.Diagonostics];
        }

        return [];
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
    
    private static void DumpLowered(string outputPath, IR_Program program) {
        var dumpPath = Path.ChangeExtension(outputPath, ".dump.sg");
        using var writer = new StreamWriter(dumpPath);

        writer.WriteLine($"// <generated dump from source file: {Path.ChangeExtension(outputPath, ".sg")}>");
        writer.WriteLine("// THIS PROGRAM CANNOT BE COMPILED");
        writer.WriteLine();

        foreach (var (header, body) in program.Functions) {
            header.WriteTo(writer);
            body.WriteTo(writer);
            writer.WriteLine();
        }
    }

    private static void DumpControlflow(string outputPath, IR_Program program) {
        foreach (var (header, body) in program.Functions) {
            
            var controlFlowStatments = !body.Statements.Any() && !program.Functions.IsEmpty
                ? program.Functions.Last().Value
                : body;

            var dumpPath = Path.ChangeExtension(
                Path.Join(Path.GetDirectoryName(outputPath), header.Name), 
                ".dot"
            );

            var controlFlowGraph = ControlFlowGraph.Create(controlFlowStatments);
            using var writer = new StreamWriter(dumpPath);
            
            controlFlowGraph.WriteTo(writer);            
        }
    }

    private static void DumpSymbols(IR_Program program) {
        foreach (var symbol in program.Symbols) {
            symbol.WriteTo(Console.Out);
            Console.WriteLine();
        }
    }

    internal IR_GlobalScope? GlobalScope {
        get {
            if (globalScope is null) {
                var prevGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref globalScope, prevGlobalScope, null);
            }
            return globalScope;
        }
    }

    public Compilation? Previous { get; }
    public FunctionSymbol? MainFunction => GlobalScope.MainFunction;
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    private IR_GlobalScope? globalScope;
}