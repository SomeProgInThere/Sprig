using System.Collections.Immutable;

using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal sealed class BoundScope(BoundScope? parent = null) {

    public bool TryDeclare(VariableSymbol variable) {
        if (variables.ContainsKey(variable.Name))
            return false;
        
        variables.Add(variable.Name, variable);
        return true;
    }

    public bool TryLookup(string name, out VariableSymbol? variable) {
        if (variables.TryGetValue(name, out variable))
            return true;

        return Parent is not null && Parent.TryLookup(name, out variable);
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