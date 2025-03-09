namespace Sprig.Codegen.Syntax;

public abstract class Clause : SyntaxNode {}

public sealed class TypeClause(SyntaxToken colonToken, SyntaxToken identifier)
    : Clause {

    public SyntaxToken ColonToken { get; } = colonToken;
    public SyntaxToken Identifier { get; } = identifier;

    public override SyntaxKind Kind => SyntaxKind.TypeClause;
}

public sealed class ElseClause(SyntaxToken elseToken, Statement body) 
    : Clause {
        
    public SyntaxToken ElseToken { get; } = elseToken;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.ElseClause;
}