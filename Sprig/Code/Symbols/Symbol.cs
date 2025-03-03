namespace Sprig.Code.Symbols;

public enum SymbolKind {
    Type,
    Variable,
    Parameter,
    Function,
}

public abstract class Symbol {
    private protected Symbol(string name) => Name = name;

    public abstract SymbolKind Kind { get; }
    public string Name { get; }

    public override string ToString() => Name;
}

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