using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.Binding;

internal sealed class BoundScope(BoundScope? parent = null) {

    public bool TryDeclareVariable(VariableSymbol variable) {
        variables ??= [];
        if (variables.ContainsKey(variable.Name))
            return false;
        
        variables.Add(variable.Name, variable);
        return true;
    }

    public bool TryLookupVariable(string name, out VariableSymbol? variable) {
        variable = null;
        if (variables != null && variables.TryGetValue(name, out variable))
            return true;

        return Parent != null && Parent.TryLookupVariable(name, out variable);
    }

    public bool TryDeclareFunction(FunctionSymbol function) {
        functions ??= [];
        if (functions.ContainsKey(function.Name))
            return false;
        
        functions.Add(function.Name, function);
        return true;
    }

    public bool TryLookupFunction(string name, out FunctionSymbol? function) {
        function = null;
        if (functions != null && functions.TryGetValue(name, out function))
            return true;

        return Parent is not null && Parent.TryLookupFunction(name, out function);
    }

    public ImmutableArray<VariableSymbol> Variables => variables is null ? [] : [..variables.Values];
    public ImmutableArray<FunctionSymbol> Functions => functions is null ? [] : [..functions.Values];

    public BoundScope? Parent { get; } = parent;

    private Dictionary<string, VariableSymbol>? variables;
    private Dictionary<string, FunctionSymbol>? functions;
}

internal sealed class BoundGlobalScope(
    BoundGlobalScope? previous,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableArray<FunctionSymbol> functions,
    ImmutableArray<VariableSymbol> variables,
    BoundBlockStatement statement
) {
    public BoundGlobalScope? Previous { get; } = previous;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableArray<FunctionSymbol> Functions { get; } = functions;
    public ImmutableArray<VariableSymbol> Variables { get; } = variables;
    public BoundBlockStatement Statement { get; } = statement;
}

internal sealed class BoundProgram(
    BoundGlobalScope globalScope,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions
) {
    public BoundGlobalScope GlobalScope { get; } = globalScope;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; } = functions;
}