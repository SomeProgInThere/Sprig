
using System.Collections.Immutable;
using Sprig.Code.Syntax;

namespace Sprig.Code.Binding;

internal abstract class BoundStatement : BoundNode {}

internal sealed class BoundBlockStatement(ImmutableArray<BoundStatement> statements)
    : BoundStatement {
        
    public ImmutableArray<BoundStatement> Statements { get; } = statements;
    public override BoundKind Kind => BoundKind.BlockStatement;
}

internal sealed class BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression initializer)
    : BoundStatement {
        
    public VariableSymbol Variable { get; } = variable;
    public BoundExpression Initializer { get; } = initializer;

    public override BoundKind Kind => BoundKind.VariableDeclarationStatement;
}

internal sealed class BoundAssignOperationStatement(VariableSymbol? variable, Token assignOperatorToken, BoundExpression expression)
    : BoundStatement {
        
    public VariableSymbol? Variable { get; } = variable;
    public Token AssignOperatorToken { get; } = assignOperatorToken;
    public BoundExpression Expression { get; } = expression;

    public override BoundKind Kind => BoundKind.AssignOperationStatement;
}

internal sealed class BoundExpressionStatement(BoundExpression expression)
    : BoundStatement {
        
    public BoundExpression Expression { get; } = expression;
    public override BoundKind Kind => BoundKind.ExpressionStatement;
}

internal sealed class BoundIfStatement(BoundExpression condition, BoundStatement ifStatement, BoundStatement? elseStatement)
    : BoundStatement {

    public BoundExpression Condition { get; } = condition;
    public BoundStatement IfStatement { get; } = ifStatement;
    public BoundStatement? ElseStatement { get; } = elseStatement;

    public override BoundKind Kind => BoundKind.IfStatement;
}

internal class BoundWhileStatement(BoundExpression condition, BoundStatement body) 
: BoundStatement {

    public BoundExpression Condition { get; } = condition;
    public BoundStatement Body { get; } = body;

    public override BoundKind Kind => BoundKind.WhileStatement;
}

internal class BoundForStatement(VariableSymbol variable, BoundExpression range, BoundStatement body)
    : BoundStatement {

    public VariableSymbol Variable { get; } = variable;
    public BoundExpression Range { get; } = range;
    public BoundStatement Body { get; } = body;

    public override BoundKind Kind => BoundKind.ForStatement;
}