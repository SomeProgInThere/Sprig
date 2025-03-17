using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IRGeneration;

internal abstract class IRStatement : IRNode {}

internal sealed class IRBlockStatement(ImmutableArray<IRStatement> statements)
    : IRStatement {
        
    public ImmutableArray<IRStatement> Statements { get; } = statements;
    public override IRNodeKind Kind => IRNodeKind.BlockStatement;
}

internal sealed class IRVariableDeclaration(VariableSymbol variable, IRExpression initializer)
    : IRStatement {
        
    public VariableSymbol Variable { get; } = variable;
    public IRExpression Initializer { get; } = initializer;

    public override IRNodeKind Kind => IRNodeKind.VariableDeclaration;
}

internal sealed class IRExpressionStatement(IRExpression expression)
    : IRStatement {
        
    public IRExpression Expression { get; } = expression;
    public override IRNodeKind Kind => IRNodeKind.ExpressionStatement;
}

internal sealed class IRGotoStatement(LabelSymbol label)
    : IRStatement {

    public LabelSymbol Label { get; } = label;
    public override IRNodeKind Kind => IRNodeKind.GotoStatement;
}

internal sealed class IRConditionalGotoStatement(LabelSymbol label, IRExpression condition, bool jump = true)
    : IRStatement {

    public LabelSymbol Label { get; } = label;
    public IRExpression Condition { get; } = condition;
    public bool Jump { get; } = jump;

    public override IRNodeKind Kind => IRNodeKind.ConditionalGotoStatement;
}

internal sealed class IRLabelStatement(LabelSymbol label)
    : IRStatement {

    public LabelSymbol Label { get; } = label;
    public override IRNodeKind Kind => IRNodeKind.LabelStatement;
}

internal sealed class IRIfStatement(IRExpression condition, IRStatement ifStatement, IRStatement? elseStatement)
    : IRStatement {

    public IRExpression Condition { get; } = condition;
    public IRStatement IfStatement { get; } = ifStatement;
    public IRStatement? ElseStatement { get; } = elseStatement;

    public override IRNodeKind Kind => IRNodeKind.IfStatement;
}

internal sealed class IRReturnStatment(IRExpression? expression) 
    : IRStatement {

    public IRExpression? Expression { get; } = expression;
    public override IRNodeKind Kind => IRNodeKind.ReturnStatement;
}