
using System.Collections.Immutable;

namespace Rubics.Code.Binding;

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

    public override BoundKind Kind => BoundKind.VariableDeclaration;
}

internal sealed class BoundExpressionStatement(BoundExpression expression)
    : BoundStatement {
        
    public BoundExpression Expression { get; } = expression;
    public override BoundKind Kind => BoundKind.ExpressionStatement;
}