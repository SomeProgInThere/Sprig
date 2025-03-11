using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.Binding;

internal sealed class BoundScope(BoundScope? parent = null) {

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

    public BoundScope? Parent { get; } = parent;
    private Dictionary<string, Symbol>? symbols;
}

internal sealed class BoundGlobalScope(
    BoundGlobalScope? previous,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableArray<Symbol> symbols,
    BoundBlockStatement statement
) {
    public BoundGlobalScope? Previous { get; } = previous;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableArray<Symbol> Symbols { get; } = symbols;
    public BoundBlockStatement Statement { get; } = statement;
}

internal sealed class BoundProgram(
    BoundGlobalScope globalScope,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions
) {
    public BoundGlobalScope GlobalScope { get; } = globalScope;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; } = functions;
}