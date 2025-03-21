using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IR;

public class Casting {

    public static readonly Casting None = new(false, false, false);
    public static readonly Casting Identity = new(true, true, true);
    public static readonly Casting Implicit = new(true, false, true);
    public static readonly Casting Explicit = new(true, false, false);
    
    private Casting(bool exists, bool isIdentity, bool isImplicit) {
        Exists = exists;
        IsIdentity = isIdentity;
        IsImplicit = isImplicit;
    }

    public bool Exists { get; }
    public bool IsIdentity { get; }
    public bool IsImplicit { get; }

    public bool IsExplicit => Exists && !IsImplicit;

    public static Casting TypeOf(TypeSymbol from, TypeSymbol to) {
        if (from == to)
            return Identity;

        if (from != TypeSymbol.Void && to == TypeSymbol.Any)
            return Implicit;

        if (from == TypeSymbol.Int32) {
            if (to == TypeSymbol.Double) {
                return Implicit;
            }
        }

        if (from == TypeSymbol.Any && to != TypeSymbol.Void)
            return Explicit;

        if (from == TypeSymbol.Boolean && to == TypeSymbol.Int32)
            return Explicit;
        
        if (from == TypeSymbol.Boolean || from == TypeSymbol.Int32 || from == TypeSymbol.Double) {
            if (to == TypeSymbol.String)
                return Explicit;

            if (to == TypeSymbol.Int32) {
                return Explicit;
            }
        }
        
        return None;
    }
}