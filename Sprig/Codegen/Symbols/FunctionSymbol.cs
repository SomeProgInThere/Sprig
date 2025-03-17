using System.Collections.Immutable;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Symbols;

public sealed class ParameterSymbol(string name, TypeSymbol type, int index)
    : VariableSymbol(name, true, type, VariableScope.Local) {
    
    public int Index { get; } = index;
    
    public override SymbolKind Kind => SymbolKind.Parameter;
}

public sealed class FunctionSymbol(
    string name, 
    ImmutableArray<ParameterSymbol> parameters, 
    TypeSymbol returnType, 
    FunctionHeader? header = null
) : Symbol(name) {
    
    public ImmutableArray<ParameterSymbol> Parameters { get; } = parameters;
    public TypeSymbol Type { get; } = returnType;
    public FunctionHeader? Header { get; } = header;

    public override SymbolKind Kind => SymbolKind.Function;
}