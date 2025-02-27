namespace Sprig.Code;

public enum SymbolKind {
    Type,
    Variable,
}

public abstract class Symbol {
    private protected Symbol(string name) => Name = name;

    public abstract SymbolKind Kind { get; }
    public string Name { get; }

    public override string ToString() => Name;
}

public sealed class TypeSymbol : Symbol {
    
    public static readonly TypeSymbol Boolean = new("Boolean");
    public static readonly TypeSymbol Int = new("Int");
    public static readonly TypeSymbol String = new("String");
    public static readonly TypeSymbol Error = new("ErrorType");

    private TypeSymbol(string name) 
        : base(name) 
    {}

    public bool IsError => this == Error;
    
    public override SymbolKind Kind => SymbolKind.Type;
}

public sealed class VariableSymbol 
    : Symbol {
    
    internal VariableSymbol(string name, bool mutable, TypeSymbol type) 
        : base(name) {
        
        Mutable = mutable;
        Type = type;
    }

    public bool Mutable { get; }
    public TypeSymbol Type { get; }

    public override SymbolKind Kind => SymbolKind.Variable;
}