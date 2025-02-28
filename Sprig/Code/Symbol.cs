using System.Collections.Immutable;

namespace Sprig.Code;

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

public sealed class TypeSymbol : Symbol {
    
    public static readonly TypeSymbol Boolean = new("boolean");
    public static readonly TypeSymbol Int = new("int");
    public static readonly TypeSymbol String = new("string");

    public static readonly TypeSymbol Void = new("void");
    public static readonly TypeSymbol Error = new("error-type");

    private TypeSymbol(string name) 
        : base(name) 
    {}

    public bool IsError => this == Error;
    
    public override SymbolKind Kind => SymbolKind.Type;
}

public class VariableSymbol 
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

public sealed class ParameterSymbol(string name, TypeSymbol type)
    : VariableSymbol(name, true, type) {
    
    public override SymbolKind Kind => SymbolKind.Parameter;
}

public sealed class FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type)
    : Symbol(name) {
    
    public ImmutableArray<ParameterSymbol> Parameters { get; } = parameters;
    public TypeSymbol Type { get; } = type;
    
    public override SymbolKind Kind => SymbolKind.Function;
}