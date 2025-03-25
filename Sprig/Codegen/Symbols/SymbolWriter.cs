
using Sprig.Codegen.Syntax;
using Sprig.IO;

namespace Sprig.Codegen.Symbols;

internal static class SymbolWriter {

    public static void WriteTo(Symbol symbol, TextWriter writer) {
        switch (symbol.Kind) {
            case SymbolKind.Type:
                WriteTypeSymbol((TypeSymbol)symbol, writer);
                break;
            
            case SymbolKind.Variable:
                WriteVariableSymbol((VariableSymbol)symbol, writer);
                break;
            
            case SymbolKind.Parameter:
                WriteParameterSymbol((ParameterSymbol)symbol, writer);
                break;
            
            case SymbolKind.Function:
                WriteFunctionSymbol((FunctionSymbol)symbol, writer);
                break;
            
            default:
                throw new Exception($"Unexpected symbol: {symbol.Kind}");
        }
    }

    private static void WriteTypeSymbol(TypeSymbol symbol, TextWriter writer) => writer.Write(symbol.Name);

    private static void WriteVariableSymbol(VariableSymbol symbol, TextWriter writer) {
        var scope = symbol.Scope == VariableScope.Local ? "local" : "global";
        writer.Write($"({scope})");
        writer.WriteSpace();

        writer.Write(symbol.Mutable ? SyntaxKind.VarKeyword : SyntaxKind.LetKeyword);
        writer.WriteSpace();
        
        writer.Write(symbol.Name);
        writer.WritePunctuation(SyntaxKind.ColonToken);
        writer.WriteSpace();
        symbol.Type.WriteTo(writer);
    }

    private static void WriteParameterSymbol(ParameterSymbol symbol, TextWriter writer) {
        writer.Write(symbol.Name);
        writer.WritePunctuation(SyntaxKind.ColonToken);
        writer.WriteSpace();
        symbol.Type.WriteTo(writer);
    }

    private static void WriteFunctionSymbol(FunctionSymbol symbol, TextWriter writer) {
        writer.Write(SyntaxKind.FuncKeyword);
        writer.WriteSpace();

        writer.Write(symbol.Name);
        writer.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        
        for (int i = 0; i < symbol.Parameters.Length; i++) {
            if (i > 0) {
                writer.WritePunctuation(SyntaxKind.CommaToken);
                writer.WriteSpace();
            }

            symbol.Parameters[i].WriteTo(writer);
        }

        writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken);
        
        if (symbol.Type != TypeSymbol.Void) {
            writer.WritePunctuation(SyntaxKind.ColonToken);
            writer.WriteSpace();
            symbol.Type.WriteTo(writer);
        }
    }
}