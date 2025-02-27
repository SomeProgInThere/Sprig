namespace Sprig.Code;

public sealed class VariableSymbol(string name, bool mutable, Type type) {
    public string Name { get; } = name;
    public bool Mutable { get; } = mutable;
    public Type Type { get; } = type;

    public override string ToString() => Name;
}