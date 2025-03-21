
using Sprig.Codegen.IR;

namespace Sprig.Codegen.Symbols;

public enum VariableScope {
    Local,
    Global
}

public class VariableSymbol 
    : Symbol {
    
    internal VariableSymbol(string name, bool mutable, TypeSymbol type, VariableScope scope, IR_Constant? constant)
        : base(name) {
        
        Mutable = mutable;
        Type = type;
        Scope = scope;
        Constant = mutable ? constant : null;
    }

    public bool Mutable { get; }
    public TypeSymbol Type { get; }
    public VariableScope Scope { get; }
    public IR_Constant? Constant { get; }

    public override SymbolKind Kind => SymbolKind.Variable;
}