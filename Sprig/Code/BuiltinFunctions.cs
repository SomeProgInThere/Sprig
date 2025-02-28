using System.Reflection;

namespace Sprig.Code;

internal static class BuiltinFunctions {

    public static readonly FunctionSymbol Print = new(
        "print", 
        [new ParameterSymbol("text", TypeSymbol.String)], 
        TypeSymbol.Void
    );

    public static readonly FunctionSymbol Input = new(
        "input",
        [],
        TypeSymbol.String
    );

    internal static IEnumerable<FunctionSymbol?> All() 
        => typeof(BuiltinFunctions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol?)f.GetValue(null));
}