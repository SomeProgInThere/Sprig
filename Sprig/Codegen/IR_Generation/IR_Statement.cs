using System.Collections.Immutable;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IR_Generation;

internal abstract class IR_Statement : IR_Node {}

internal sealed class IR_BlockStatement(ImmutableArray<IR_Statement> statements)
    : IR_Statement {
        
    public ImmutableArray<IR_Statement> Statements { get; } = statements;
    public override IR_NodeKind Kind => IR_NodeKind.BlockStatement;
}

internal sealed class IR_VariableDeclaration(VariableSymbol variable, IR_Expression initializer)
    : IR_Statement {
        
    public VariableSymbol Variable { get; } = variable;
    public IR_Expression Initializer { get; } = initializer;

    public override IR_NodeKind Kind => IR_NodeKind.VariableDeclaration;
}

internal sealed class IR_ExpressionStatement(IR_Expression expression)
    : IR_Statement {
        
    public IR_Expression Expression { get; } = expression;
    public override IR_NodeKind Kind => IR_NodeKind.ExpressionStatement;
}

internal sealed class IR_GotoStatement(LabelSymbol label)
    : IR_Statement {

    public LabelSymbol Label { get; } = label;
    public override IR_NodeKind Kind => IR_NodeKind.GotoStatement;
}

internal sealed class IR_ConditionalGotoStatement(LabelSymbol label, IR_Expression condition, bool jump = true)
    : IR_Statement {

    public LabelSymbol Label { get; } = label;
    public IR_Expression Condition { get; } = condition;
    public bool Jump { get; } = jump;

    public override IR_NodeKind Kind => IR_NodeKind.ConditionalGotoStatement;
}

internal sealed class IR_LabelStatement(LabelSymbol label)
    : IR_Statement {

    public LabelSymbol Label { get; } = label;
    public override IR_NodeKind Kind => IR_NodeKind.LabelStatement;
}

internal sealed class IR_IfStatement(IR_Expression condition, IR_Statement ifStatement, IR_Statement? elseStatement)
    : IR_Statement {

    public IR_Expression Condition { get; } = condition;
    public IR_Statement IfStatement { get; } = ifStatement;
    public IR_Statement? ElseStatement { get; } = elseStatement;

    public override IR_NodeKind Kind => IR_NodeKind.IfStatement;
}

internal sealed class IR_ReturnStatment(IR_Expression? expression) 
    : IR_Statement {

    public IR_Expression? Expression { get; } = expression;
    public override IR_NodeKind Kind => IR_NodeKind.ReturnStatement;
}