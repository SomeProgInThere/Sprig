namespace Sprig.Codegen.Symbols;

internal sealed class LabelSymbol(string name) {
    public string Name { get; } = name;

    public override string ToString() => Name;
}