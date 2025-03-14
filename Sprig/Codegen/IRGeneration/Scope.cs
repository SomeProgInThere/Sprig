using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IRGeneration;

internal sealed class LocalScope(LocalScope? parent = null) {

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

    public LocalScope? Parent { get; } = parent;
    private Dictionary<string, Symbol>? symbols;
}

internal sealed class GlobalScope(
    GlobalScope? previous,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableArray<Symbol> symbols,
    ImmutableArray<IRStatement> statements
) {
    public GlobalScope? Previous { get; } = previous;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableArray<Symbol> Symbols { get; } = symbols;
    public ImmutableArray<IRStatement> Statements { get; } = statements;
}

internal sealed class IRProgram(
    IRProgram previous,
    IRBlockStatement statement,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableDictionary<FunctionSymbol, IRBlockStatement> functions
) {
    public IRProgram Previous { get; } = previous;
    public IRBlockStatement Statement { get; } = statement;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableDictionary<FunctionSymbol, IRBlockStatement> Functions { get; } = functions;
}