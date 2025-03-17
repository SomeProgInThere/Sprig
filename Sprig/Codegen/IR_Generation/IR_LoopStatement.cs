using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IR_Generation;

internal sealed class JumpLabel(LabelSymbol brakeLabel, LabelSymbol continueLabel) {
    public LabelSymbol? BrakeLabel { get; } = brakeLabel;
    public LabelSymbol? ContinueLabel { get; } = continueLabel;
}

internal abstract class IR_LoopStatement(JumpLabel jumpLabel)
    : IR_Statement {

    public JumpLabel JumpLabel { get; } = jumpLabel;
}

internal class IR_WhileStatement(IR_Expression condition, IR_Statement body, JumpLabel jumpLabel)
    : IR_LoopStatement(jumpLabel) {
    
    public IR_Expression Condition { get; } = condition;
    public IR_Statement Body { get; } = body;

    public override IR_NodeKind Kind => IR_NodeKind.WhileStatement;
}

internal class IR_DoWhileStatement(IR_Statement body, IR_Expression condition, JumpLabel jumpLabel) 
    : IR_LoopStatement(jumpLabel) {

    public IR_Statement Body { get; } = body;
    public IR_Expression Condition { get; } = condition;

    public override IR_NodeKind Kind => IR_NodeKind.DoWhileStatement;
}

internal class IR_ForStatement(VariableSymbol variable, IR_Expression range, IR_Statement body, JumpLabel jumpLabel)
    : IR_LoopStatement(jumpLabel) {

    public VariableSymbol Variable { get; } = variable;
    public IR_Expression Range { get; } = range;
    public IR_Statement Body { get; } = body;

    public override IR_NodeKind Kind => IR_NodeKind.ForStatement;
}