namespace Sprig.Codegen.Syntax;

public abstract class Clause(SyntaxTree syntaxTree) 
    : SyntaxNode(syntaxTree) {}

public sealed class TypeClause(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier)
    : Clause(syntaxTree) {

    public SyntaxToken ColonToken { get; } = colonToken;
    public SyntaxToken Identifier { get; } = identifier;

    public override SyntaxKind Kind => SyntaxKind.TypeClause;
}

public sealed class ElseClause(SyntaxTree syntaxTree, SyntaxToken elseToken, Statement body) 
    : Clause(syntaxTree) {
        
    public SyntaxToken ElseToken { get; } = elseToken;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.ElseClause;
}