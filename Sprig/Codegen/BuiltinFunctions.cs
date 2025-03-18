using System.Reflection;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

internal static class BuiltinFunctions {

    public static readonly FunctionSymbol Print = new(
        "print", 
        [new ParameterSymbol("arg", TypeSymbol.String, 0)], 
        TypeSymbol.Void
    );

    public static readonly FunctionSymbol Input = new(
        "input",
        [],
        TypeSymbol.String
    );

    public static readonly FunctionSymbol RandInt = new(
        "rand",
        [
            new ParameterSymbol("min", TypeSymbol.Int32, 0), 
            new ParameterSymbol("max", TypeSymbol.Int32, 0),
        ], 
        TypeSymbol.Int32
    );
    
    internal static IEnumerable<FunctionSymbol?> All()
        => typeof(BuiltinFunctions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol?)f.GetValue(null));
}