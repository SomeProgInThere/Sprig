using System.Collections.Immutable;
using Sprig.Codegen.Syntax;

namespace Sprig.Codegen.Symbols;

public sealed class ParameterSymbol(string name, TypeSymbol type)
    : VariableSymbol(name, true, type, VariableScope.Local) {
    
    public override SymbolKind Kind => SymbolKind.Parameter;
}

public sealed class FunctionSymbol(
    string name, 
    ImmutableArray<ParameterSymbol> parameters, 
    TypeSymbol returnType, 
    FunctionHeader? header = null
) : Symbol(name) {
    
    public ImmutableArray<ParameterSymbol> Parameters { get; } = parameters;
    public TypeSymbol ReturnType { get; } = returnType;
    public FunctionHeader? Header { get; } = header;

    public override SymbolKind Kind => SymbolKind.Function;
}