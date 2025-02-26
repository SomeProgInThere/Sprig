using System.Collections.Immutable;

namespace Sprig.Code.Syntax;

public abstract class Statement : SyntaxNode {}

public sealed class BlockStatement(SyntaxToken openBraceToken, ImmutableArray<Statement> statements, SyntaxToken closedBraceToken)
    : Statement {

    public SyntaxToken OpenBraceToken { get; } = openBraceToken;
    public ImmutableArray<Statement> Statements { get; } = statements;
    public SyntaxToken ClosedBraceToken { get; } = closedBraceToken;

    public override SyntaxKind Kind => SyntaxKind.BlockStatment;
}

public sealed class ExpressionStatement(Expression expression)
    : Statement {
        
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
}

public sealed class VariableDeclarationStatement(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsToken, Expression initializer)
    : Statement {

    public SyntaxToken Keyword { get; } = keyword;
    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken EqualsToken { get; } = equalsToken;
    public Expression Initializer { get; } = initializer;

    public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
}

public sealed class AssignOperationStatement(SyntaxToken identifier, SyntaxToken assignOperatorToken, Expression expression)
    : Statement {

    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken AssignOperatorToken { get; } = assignOperatorToken;
    public Expression Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.AssignOperationStatement;
}

public sealed class IfStatement(SyntaxToken ifToken, Expression condition, Statement body, ElseClause? elseClause) 
    : Statement {

    public SyntaxToken IfToken { get; } = ifToken;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
    public ElseClause? ElseClause { get; } = elseClause;

    public override SyntaxKind Kind => SyntaxKind.IfStatement;
}

public sealed class WhileStatement(SyntaxToken whileToken, Expression condition, Statement body) 
    : Statement {

    public SyntaxToken WhileToken { get; } = whileToken;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
}

public sealed class ForStatement(SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken inKeyword, Expression range, Statement body) 
    : Statement {

    public SyntaxToken ForKeyword { get; } = forKeyword;
    public SyntaxToken Identifier { get; } = identifier;
    public SyntaxToken InKeyword { get; } = inKeyword;
    
    public Expression Range { get; } = range;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.ForStatement;
}
