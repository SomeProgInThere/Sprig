using System.Collections.Immutable;

namespace Sprig.Codegen.Syntax;

public abstract class Statement(SyntaxTree syntaxTree) 
    : SyntaxNode(syntaxTree) {}

public sealed class BlockStatement(
    SyntaxTree syntaxTree, 
    SyntaxToken openBraceToken, 
    ImmutableArray<Statement> statements, 
    SyntaxToken closedBraceToken
)
    : Statement(syntaxTree) {

    public SyntaxToken OpenBraceToken { get; } = openBraceToken;
    public ImmutableArray<Statement> Statements { get; } = statements;
    public SyntaxToken ClosedBraceToken { get; } = closedBraceToken;

    public override SyntaxKind Kind => SyntaxKind.BlockStatment;
}

public sealed class ExpressionStatement(SyntaxTree syntaxTree, Expression expression)
    : Statement(syntaxTree) {
        
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
}

public sealed class VariableDeclarationStatement(
    SyntaxTree syntaxTree, 
    SyntaxToken keyword, 
    SyntaxToken identifier, 
    TypeClause? typeClause,
    SyntaxToken equalsToken, 
    Expression initializer
) 
    : Statement(syntaxTree) {

    public SyntaxToken Keyword { get; } = keyword;
    public SyntaxToken Identifier { get; } = identifier;
    public TypeClause? TypeClause { get; } = typeClause;
    public SyntaxToken EqualsToken { get; } = equalsToken;
    public Expression Initializer { get; } = initializer;

    public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
}

public sealed class IfStatement(
    SyntaxTree syntaxTree, 
    SyntaxToken ifKeyword, 
    Expression condition, 
    Statement body, 
    ElseClause? elseClause
) 
    : Statement(syntaxTree) {

    public SyntaxToken IfKeyword { get; } = ifKeyword;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
    public ElseClause? ElseClause { get; } = elseClause;

    public override SyntaxKind Kind => SyntaxKind.IfStatement;
}

public sealed class WhileStatement(
    SyntaxTree syntaxTree, 
    SyntaxToken whileKeyword, 
    Expression condition, 
    Statement body
) 
    : Statement(syntaxTree) {

    public SyntaxToken WhileKeyword { get; } = whileKeyword;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
}

public sealed class DoWhileStatement(
    SyntaxTree syntaxTree, 
    SyntaxToken doKeyword, 
    Statement body, 
    SyntaxToken whileKeyword, 
    Expression condition
) 
    : Statement(syntaxTree) {

    public SyntaxToken DoKeyword { get; } = doKeyword;
    public Statement Body { get; } = body;
    public SyntaxToken WhileKeyword { get; } = whileKeyword;
    public Expression Condition { get; } = condition;

    public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
}

public sealed class ForStatement(
    SyntaxTree syntaxTree, 
    SyntaxToken forKeyword, 
    SyntaxToken identifier, 
    SyntaxToken inKeyword, 
    Expression range, 
    Statement body
) 
    : Statement(syntaxTree) {

    public SyntaxToken ForKeyword { get; } = forKeyword;
    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken InKeyword { get; } = inKeyword;
    
    public Expression Range { get; } = range;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.ForStatement;
}

public sealed class BreakStatement(SyntaxTree syntaxTree, SyntaxToken breakKeyword)
    : Statement(syntaxTree) {

    public SyntaxToken BreakKeyword { get; } = breakKeyword;
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
}

public sealed class ContinueStatement(SyntaxTree syntaxTree, SyntaxToken continueKeyword)
    : Statement(syntaxTree) {

    public SyntaxToken ContinueKeyword { get; } = continueKeyword;
    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
}

public sealed class ReturnStatement(SyntaxTree syntaxTree, SyntaxToken returnKeyword, Expression? expression) 
    : Statement(syntaxTree) {
    
    public SyntaxToken ReturnKeyword { get; } = returnKeyword;
    public Expression? Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
}