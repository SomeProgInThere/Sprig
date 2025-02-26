namespace Sprig.Code.Syntax;

public abstract class Clause : SyntaxNode {}

public sealed class ElseClause(SyntaxToken elseToken, Statement body) 
    : Clause {
        
    public SyntaxToken ElseToken { get; } = elseToken;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.ElseClause;
}