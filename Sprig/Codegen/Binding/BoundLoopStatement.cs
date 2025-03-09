using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.Binding;

internal sealed class JumpLabel(LabelSymbol brakeLabel, LabelSymbol continueLabel) {
    public LabelSymbol? BrakeLabel { get; } = brakeLabel;
    public LabelSymbol? ContinueLabel { get; } = continueLabel;
}

internal abstract class BoundLoopStatement(JumpLabel jumpLabel)
    : BoundStatement {

    public JumpLabel JumpLabel { get; } = jumpLabel;
}

internal class BoundWhileStatement(BoundExpression condition, BoundStatement body, JumpLabel jumpLabel)
    : BoundLoopStatement(jumpLabel) {
    
    public BoundExpression Condition { get; } = condition;
    public BoundStatement Body { get; } = body;

    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
}

internal class BoundDoWhileStatement(BoundStatement body, BoundExpression condition, JumpLabel jumpLabel) 
    : BoundLoopStatement(jumpLabel) {

    public BoundStatement Body { get; } = body;
    public BoundExpression Condition { get; } = condition;

    public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;
}

internal class BoundForStatement(VariableSymbol variable, BoundExpression range, BoundStatement body, JumpLabel jumpLabel)
    : BoundLoopStatement(jumpLabel) {

    public VariableSymbol Variable { get; } = variable;
    public BoundExpression Range { get; } = range;
    public BoundStatement Body { get; } = body;

    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
}