using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IRGeneration;

internal sealed class JumpLabel(LabelSymbol brakeLabel, LabelSymbol continueLabel) {
    public LabelSymbol? BrakeLabel { get; } = brakeLabel;
    public LabelSymbol? ContinueLabel { get; } = continueLabel;
}

internal abstract class IRLoopStatement(JumpLabel jumpLabel)
    : IRStatement {

    public JumpLabel JumpLabel { get; } = jumpLabel;
}

internal class IRWhileStatement(IRExpression condition, IRStatement body, JumpLabel jumpLabel)
    : IRLoopStatement(jumpLabel) {
    
    public IRExpression Condition { get; } = condition;
    public IRStatement Body { get; } = body;

    public override IRNodeKind Kind => IRNodeKind.WhileStatement;
}

internal class IRDoWhileStatement(IRStatement body, IRExpression condition, JumpLabel jumpLabel) 
    : IRLoopStatement(jumpLabel) {

    public IRStatement Body { get; } = body;
    public IRExpression Condition { get; } = condition;

    public override IRNodeKind Kind => IRNodeKind.DoWhileStatement;
}

internal class IRForStatement(VariableSymbol variable, IRExpression range, IRStatement body, JumpLabel jumpLabel)
    : IRLoopStatement(jumpLabel) {

    public VariableSymbol Variable { get; } = variable;
    public IRExpression Range { get; } = range;
    public IRStatement Body { get; } = body;

    public override IRNodeKind Kind => IRNodeKind.ForStatement;
}