
namespace Rubics.Code.Syntax;

public abstract class Clause : SyntaxNode {}

public sealed class ElseClause(Token elseToken, Statement body) 
    : Clause {
        
    public Token ElseToken { get; } = elseToken;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.ElseClause;
}