using Sprig.Code.Symbols;

namespace Sprig.Code.Binding;

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
        
        if (from == TypeSymbol.Bool || from == TypeSymbol.Int) {
            if (to == TypeSymbol.String)
                return Explicit;
        }

        if (from == TypeSymbol.String) {
            if (to == TypeSymbol.Bool || to == TypeSymbol.Int)
                return Explicit;
        }
        
        return None;
    }
}