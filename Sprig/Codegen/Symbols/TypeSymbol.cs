namespace Sprig.Codegen.Symbols;

public sealed class TypeSymbol : Symbol {
    
    public static readonly TypeSymbol Boolean = new("bool");
    public static readonly TypeSymbol Int32 = new("int");
    public static readonly TypeSymbol Double = new("decimal");
    public static readonly TypeSymbol String = new("string");

    public static readonly TypeSymbol Any = new("any");
    public static readonly TypeSymbol Void = new("void");
    public static readonly TypeSymbol Error = new("error");

    public bool IsError => this == Error;

    private TypeSymbol(string name) 
        : base(name) 
    {}
    
    public override SymbolKind Kind => SymbolKind.Type;
}