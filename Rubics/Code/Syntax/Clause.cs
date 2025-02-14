
namespace Rubics.Code.Syntax;

public abstract class Clause : SyntaxNode {}

public sealed class IfClause(Token ifToken, Expression condition, Statement ifStatment) 
    : Clause {
        
    public Token IfToken { get; } = ifToken;
    public Expression Condition { get; } = condition;
    public Statement IfStatment { get; } = ifStatment;

    public override SyntaxKind Kind => SyntaxKind.IfClause;
}

public sealed class ElseClause(Token elseToken, Statement elseStatment) 
    : Clause {
        
    public Token ElseToken { get; } = elseToken;
    public Statement ElseStatment { get; } = elseStatment;

    public override SyntaxKind Kind => SyntaxKind.ElseClause;
}