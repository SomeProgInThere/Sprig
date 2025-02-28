using System.Collections.Immutable;

namespace Sprig.Code.Symbols;

public sealed class ParameterSymbol(string name, TypeSymbol type)
    : VariableSymbol(name, true, type) {
    
    public override SymbolKind Kind => SymbolKind.Parameter;
}

public sealed class FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type)
    : Symbol(name) {
    
    public ImmutableArray<ParameterSymbol> Parameters { get; } = parameters;
    public TypeSymbol Type { get; } = type;
    
    public override SymbolKind Kind => SymbolKind.Function;
}