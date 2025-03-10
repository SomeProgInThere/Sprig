using System.Collections.Immutable;

namespace Sprig.Codegen.Syntax;

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

public sealed class VariableDeclarationStatement(
    SyntaxToken keyword, SyntaxToken identifier, TypeClause? typeClause, SyntaxToken equalsToken, Expression initializer
) : Statement {

    public SyntaxToken Keyword { get; } = keyword;
    public SyntaxToken Identifier { get; } = identifier;
    public TypeClause? TypeClause { get; } = typeClause;
    public SyntaxToken EqualsToken { get; } = equalsToken;
    public Expression Initializer { get; } = initializer;

    public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
}

public sealed class IfStatement(SyntaxToken ifKeyword, Expression condition, Statement body, ElseClause? elseClause) 
    : Statement {

    public SyntaxToken IfKeyword { get; } = ifKeyword;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;
    public ElseClause? ElseClause { get; } = elseClause;

    public override SyntaxKind Kind => SyntaxKind.IfStatement;
}

public sealed class WhileStatement(SyntaxToken whileKeyword, Expression condition, Statement body) 
    : Statement {

    public SyntaxToken WhileKeyword { get; } = whileKeyword;
    public Expression Condition { get; } = condition;
    public Statement Body { get; } = body;

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;
}

public sealed class DoWhileStatement(SyntaxToken doKeyword, Statement body, SyntaxToken whileKeyword, Expression condition) 
    : Statement {

    public SyntaxToken DoKeyword { get; } = doKeyword;
    public Statement Body { get; } = body;
    public SyntaxToken WhileKeyword { get; } = whileKeyword;
    public Expression Condition { get; } = condition;

    public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
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

public sealed class BreakStatement(SyntaxToken breakKeyword)
    : Statement {

    public SyntaxToken BreakKeyword { get; } = breakKeyword;
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
}

public sealed class ContinueStatement(SyntaxToken continueKeyword)
    : Statement {

    public SyntaxToken ContinueKeyword { get; } = continueKeyword;
    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
}

public sealed class ReturnStatement(SyntaxToken returnKeyword, Expression? expression) 
    : Statement {
        
    public SyntaxToken ReturnKeyword { get; } = returnKeyword;
    public Expression? Expression { get; } = expression;

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
}