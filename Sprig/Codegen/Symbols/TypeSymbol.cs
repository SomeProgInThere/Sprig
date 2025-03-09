namespace Sprig.Codegen.Symbols;

public sealed class TypeSymbol : Symbol {
    
    public static readonly TypeSymbol Bool = new("bool");
    public static readonly TypeSymbol Int = new("int");
    public static readonly TypeSymbol Float = new("float");
    public static readonly TypeSymbol String = new("string");

    public static readonly TypeSymbol Void = new("void");
    public static readonly TypeSymbol Error = new("error-type");

    public bool IsError => this == Error;

    private TypeSymbol(string name) 
        : base(name) 
    {}
    
    public override SymbolKind Kind => SymbolKind.Type;
}