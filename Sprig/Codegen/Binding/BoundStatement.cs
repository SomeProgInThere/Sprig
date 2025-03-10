using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.Binding;

internal abstract class BoundStatement : BoundNode {}

internal sealed class BoundBlockStatement(ImmutableArray<BoundStatement> statements)
    : BoundStatement {
        
    public ImmutableArray<BoundStatement> Statements { get; } = statements;
    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
}

internal sealed class BoundVariableDeclaration(VariableSymbol variable, BoundExpression initializer)
    : BoundStatement {
        
    public VariableSymbol Variable { get; } = variable;
    public BoundExpression Initializer { get; } = initializer;

    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;
}

internal sealed class BoundExpressionStatement(BoundExpression expression)
    : BoundStatement {
        
    public BoundExpression Expression { get; } = expression;
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
}

internal sealed class BoundGotoStatement(LabelSymbol label)
    : BoundStatement {

    public LabelSymbol Label { get; } = label;
    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
}

internal sealed class BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jump = true)
    : BoundStatement {

    public LabelSymbol Label { get; } = label;
    public BoundExpression Condition { get; } = condition;
    public bool Jump { get; } = jump;

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
}

internal sealed class BoundLabelStatement(LabelSymbol label)
    : BoundStatement {

    public LabelSymbol Label { get; } = label;
    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
}

internal sealed class BoundIfStatement(BoundExpression condition, BoundStatement ifStatement, BoundStatement? elseStatement)
    : BoundStatement {

    public BoundExpression Condition { get; } = condition;
    public BoundStatement IfStatement { get; } = ifStatement;
    public BoundStatement? ElseStatement { get; } = elseStatement;

    public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
}

internal sealed class BoundReturnStatment(BoundExpression expression) 
    : BoundStatement {

    public BoundExpression Expression { get; } = expression;
    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
}