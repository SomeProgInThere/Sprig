
namespace Sprig.Codegen.Symbols;

public enum VariableScope {
    Local,
    Global
}

public class VariableSymbol 
    : Symbol {
    
    internal VariableSymbol(string name, bool mutable, TypeSymbol type, VariableScope scope)
        : base(name) {
        
        Mutable = mutable;
        Type = type;
        Scope = scope;
    }

    public bool Mutable { get; }
    public TypeSymbol Type { get; }
    public VariableScope Scope { get; }

    public override SymbolKind Kind => SymbolKind.Variable;
}