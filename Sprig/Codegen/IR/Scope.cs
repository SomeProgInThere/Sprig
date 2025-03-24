using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IR;

internal sealed class IR_LocalScope(IR_LocalScope? parent = null) {

    public bool TryDeclareSymbol(Symbol symbol) {
        symbols ??= [];
        if (symbols.ContainsKey(symbol.Name))
            return false;

        symbols.Add(symbol.Name, symbol);
        return true;
    }

    public Symbol? TryLookupSymbol(string name) {
        if (symbols != null && symbols.TryGetValue(name, out var symbol))
            return symbol;

        return Parent?.TryLookupSymbol(name);
    }
    
    public ImmutableArray<Symbol> Symbols => symbols is null ? [] : [..symbols.Values];

    public IR_LocalScope? Parent { get; } = parent;
    private Dictionary<string, Symbol>? symbols;
}

internal sealed class IR_GlobalScope(
    IR_GlobalScope? previous,
    ImmutableArray<DiagnosticMessage> diagnostics,
    FunctionSymbol? mainFunction,
    ImmutableArray<Symbol> symbols,
    ImmutableArray<IR_Statement> statements
) {
    public IR_GlobalScope? Previous { get; } = previous;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public FunctionSymbol? MainFunction { get; } = mainFunction;
    public ImmutableArray<Symbol> Symbols { get; } = symbols;
    public ImmutableArray<IR_Statement> Statements { get; } = statements;
}

internal sealed class IR_Program(
    IR_Program previous,
    ImmutableArray<DiagnosticMessage> diagnostics,
    FunctionSymbol? mainFunction,
    ImmutableDictionary<FunctionSymbol, IR_BlockStatement> functions
) {
    public IR_Program Previous { get; } = previous;
    public FunctionSymbol? MainFunction { get; } = mainFunction;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableDictionary<FunctionSymbol, IR_BlockStatement> Functions { get; } = functions;
}