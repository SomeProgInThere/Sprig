using System.Reflection;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

internal static class BuiltinFunctions {

    public static readonly FunctionSymbol Print = new(
        "print", 
        [new ParameterSymbol("text", TypeSymbol.String, 0)], 
        TypeSymbol.Void
    );

    public static readonly FunctionSymbol Input = new(
        "input",
        [],
        TypeSymbol.String
    );

    public static readonly FunctionSymbol Random = new(
        "rand",
        [
            new ParameterSymbol("min", TypeSymbol.Int, 0), 
            new ParameterSymbol("max", TypeSymbol.Int, 0)
        ], 
        TypeSymbol.Int
    );

    internal static IEnumerable<FunctionSymbol?> All()
        => typeof(BuiltinFunctions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol?)f.GetValue(null));
}