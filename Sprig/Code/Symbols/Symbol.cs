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