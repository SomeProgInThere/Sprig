namespace Sprig.Code.Syntax;

public sealed class VariableSymbol(string name, bool mutable, Type type) {
    public string Name { get; } = name;
    public bool Mutable { get; } = mutable;
    public Type Type { get; } = type;

    public override string ToString() => Name;
}

public sealed class LabelSymbol(string name) {
    public string Name { get; } = name;

    public override string ToString() => Name;
}