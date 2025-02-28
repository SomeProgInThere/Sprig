using System.Collections.Immutable;
using Sprig.Code.Symbols;

namespace Sprig.Code.Binding;

internal sealed class BoundScope(BoundScope? parent = null) {

    public bool TryDeclareVariable(VariableSymbol variable) {
        if (variables.ContainsKey(variable.Name))
            return false;
        
        variables.Add(variable.Name, variable);
        return true;
    }

    public bool TryLookupVariable(string name, out VariableSymbol? variable) {
        if (variables.TryGetValue(name, out variable))
            return true;

        return Parent is not null && Parent.TryLookupVariable(name, out variable);
    }

    public ImmutableArray<VariableSymbol> Variables => [..variables.Values];
    public BoundScope? Parent { get; } = parent;

    private readonly Dictionary<string, VariableSymbol> variables = [];
}

internal sealed class BoundGlobalScope(
    BoundGlobalScope? previous,
    ImmutableArray<DiagnosticMessage> diagnostics,
    ImmutableArray<VariableSymbol> variables,
    BoundStatement statement
) {
    public BoundGlobalScope? Previous { get; } = previous;
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public ImmutableArray<VariableSymbol> Variables { get; } = variables;
    public BoundStatement Statement { get; } = statement;
}