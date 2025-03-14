namespace Sprig.Codegen.Symbols;

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

    public void WriteTo(TextWriter writer) {
        SymbolWriter.WriteTo(this, writer);
    }

    public override string ToString() {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}