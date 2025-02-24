
using Sprig.Code.Binding;
using Sprig.Code.Syntax;

namespace Sprig.Code.Lowering;

internal sealed class Lowerer() : BoundTreeRewriter {

    public static BoundStatement Lower(BoundStatement statement) {
        var lowerer = new Lowerer();
        return lowerer.RewriteStatement(statement);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement node) {
        var range = (BoundRangeExpression)node.Range;
        var variableDeclaration = new BoundVariableDeclarationStatement(node.Variable, range.Lower);
        var variable = new BoundVariableExpression(node.Variable);

        var condition = new BoundBinaryExpression(
            variable,
            range.Upper,
            BinaryOperator.Bind(SyntaxKind.RightArrowEqualsToken, typeof(int), typeof(int)) 
                ?? throw new Exception("Invaild binary operation")
        );

        var step = new BoundExpressionStatement(
            new BoundAssignmentExpression(node.Variable, new BoundBinaryExpression(
                    variable,
                    new BoundLiteralExpression(1),
                    BinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int))
                        ?? throw new Exception("Invalid binary operation")
                )
            )
        );

        var whileBody = new BoundBlockStatement([node.Body, step]);
        var whileStatement = new BoundWhileStatement(condition, whileBody);
        var result = new BoundBlockStatement([variableDeclaration, whileStatement]);

        return RewriteStatement(result);
    }
}