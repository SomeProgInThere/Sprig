using System.CodeDom.Compiler;
using Sprig.Code.Symbols;
using Sprig.Code.Syntax;
using Sprig.IO;

namespace Sprig.Code.Binding;

internal static class BoundNodePrinter {

    public static void WriteTo(this BoundNode node, TextWriter writer) {
        if (writer is IndentedTextWriter indentedWriter)
            WriteTo(node, indentedWriter);
        else
            WriteTo(node, new IndentedTextWriter(writer));     
    }

    public static void WriteTo(this BoundNode node, IndentedTextWriter writer) {
        switch (node.Kind) {
            case BoundNodeKind.LiteralExpression: 
                WriteLiteralExpression((BoundLiteralExpression)node, writer);
                break;
            
            case BoundNodeKind.VariableExpression: 
                WriteVariableExpression((BoundVariableExpression)node, writer);
                break;
                
            case BoundNodeKind.AssignmentExpression: 
                WriteAssignmentExpression((BoundAssignmentExpression)node, writer);
                break;
                
            case BoundNodeKind.UnaryExpression: 
                WriteUnaryExpression((BoundUnaryExpression)node, writer);
                break;
                
            case BoundNodeKind.BinaryExpression: 
                WriteBinaryExpression((BoundBinaryExpression)node, writer);
                break;
                
            case BoundNodeKind.RangeExpression: 
                WriteRangeExpression((BoundRangeExpression)node, writer);
                break;
                
            case BoundNodeKind.CallExpression: 
                WriteCallExpression((BoundCallExpression)node, writer);
                break;
                
            case BoundNodeKind.CastExpression: 
                WriteCastExpression((BoundCastExpression)node, writer);
                break;
                
            case BoundNodeKind.ErrorExpression: 
                writer.WriteKeyword("?");
                break;
                
            case BoundNodeKind.BlockStatement: 
                WriteBlockStatement((BoundBlockStatement)node, writer);
                break;
                
            case BoundNodeKind.VariableDeclaration: 
                WriteVariableDeclaration((BoundVariableDeclaration)node, writer);
            break;

            case BoundNodeKind.ExpressionStatement: 
                WriteExpressionStatement((BoundExpressionStatement)node, writer);
                break;
                
            case BoundNodeKind.GotoStatement: 
                WriteGotoStatement((BoundGotoStatement)node, writer);
                break;
                
            case BoundNodeKind.ConditionalGotoStatement: 
                WriteConditionalGoto((BoundConditionalGoto)node, writer);
                break;
                
            case BoundNodeKind.LabelStatement: 
                WriteLabelStatement((BoundLabelStatement)node, writer);
                break;
                
            case BoundNodeKind.IfStatement: 
                WriteIfStatement((BoundIfStatement)node, writer);
                break;
            
            case BoundNodeKind.WhileStatement: 
                WriteWhileStatement((BoundWhileStatement)node, writer);
                break;
                
            case BoundNodeKind.DoWhileStatement: 
                WriteDoWhileStatement((BoundDoWhileStatement)node, writer);
                break;
                
            case BoundNodeKind.ForStatement: 
                WriteForStatement((BoundForStatement)node, writer);
                break;

            default:
                throw new Exception($"Unexpected node: {node.Kind}");
        }
    }

    private static void WriteNestedExpressions(
        this IndentedTextWriter writer, 
        int parentPrecendence, 
        BoundExpression node
    ) {
        if (node is BoundUnaryExpression unary)
            writer.WriteNestedExpressions(
                parentPrecendence, 
                unary.Operator.SyntaxKind.UnaryOperatorPrecedence(), 
                unary
            );
        
        else if (node is BoundBinaryExpression binary)
            writer.WriteNestedExpressions(
                parentPrecendence, 
                binary.Operator.SyntaxKind.BinaryOperatorPrecedence(), 
                binary
            );
        
        else
            node.WriteTo(writer);
    }

    private static void WriteNestedExpressions(
        this IndentedTextWriter writer, 
        int parentPrecendence, 
        int currentPrecedence, 
        BoundExpression node
    ) {
        var needsParenthesis = parentPrecendence >= currentPrecedence;
        if (needsParenthesis)
            writer.WritePunctuation("(");
        
        node.WriteTo(writer);

        if (needsParenthesis)
            writer.WritePunctuation(")");
    }

    private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer) {
        var value = node.Value.ToString() ?? "?";
        
        if (node.Type == TypeSymbol.Bool)
            writer.WriteKeyword(value);

        else if (node.Type == TypeSymbol.Int)
            writer.WriteNumber(value);
            
        else if (node.Type == TypeSymbol.Float)
            writer.WriteNumber(value);

        else if (node.Type == TypeSymbol.String) {
            value = "\"" + value.Replace("\"", "\"\"") + "\"";
            writer.WriteString(value);
        }

        else throw new Exception($"Unexpected type: {node.Type}");
    }

    private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Variable.Name);
    }

    private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Variable.Name);
        writer.WritePunctuation(" = ");
        node.Expression.WriteTo(writer);
    }

    private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer) {
        var kind = node.Operator.SyntaxKind;
        var precedence = kind.UnaryOperatorPrecedence();
        
        writer.WritePunctuation(kind.Literal());
        writer.WriteNestedExpressions(precedence, node.Operand);
    }

    private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer) {
        var kind = node.Operator.SyntaxKind;
        var precedence = kind.BinaryOperatorPrecedence();

        writer.WriteNestedExpressions(precedence, node.Left);        
        writer.WritePunctuation(" " + kind.Literal() + " ");
        writer.WriteNestedExpressions(precedence, node.Right);        
    }

    private static void WriteRangeExpression(BoundRangeExpression node, IndentedTextWriter writer) {
        
    }

    private static void WriteCallExpression(BoundCallExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Function.Name);
        writer.WritePunctuation("(");

        var isFirst = true;
        foreach (var argument in node.Arguments) {
            if (isFirst)
                isFirst = false;
            else
                writer.WritePunctuation(", ");

            argument.WriteTo(writer);
        }

        writer.WritePunctuation(")");
    }

    private static void WriteCastExpression(BoundCastExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Type.Name);
        writer.WritePunctuation("(");
        node.Expression.WriteTo(writer);
        writer.WritePunctuation(")");
    }

    private static void WriteNestedStatements(this IndentedTextWriter writer, BoundStatement node) {
        var needsIndent = node is not BoundBlockStatement;
        if (needsIndent)
            writer.Indent++;

        node.WriteTo(writer);
        if (needsIndent)
            writer.Indent--;
    }

    private static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer) {
        writer.WritePunctuation("{");
        writer.WriteLine();
        writer.Indent++;

        foreach (var statement in node.Statements)
            statement.WriteTo(writer);

        writer.Indent--;
        writer.WritePunctuation("}");
        writer.WriteLine();
    }

    private static void WriteVariableDeclaration(BoundVariableDeclaration node, IndentedTextWriter writer) {
        writer.WriteKeyword(node.Variable.Mutable ? "var " : "let ");
        writer.WriteIdentifier(node.Variable.Name);
        writer.WritePunctuation(" = ");

        node.Initializer.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer) {
        node.Expression.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteGotoStatement(BoundGotoStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(node.Label.Name);
        writer.WriteLine();
    }

    private static void WriteConditionalGoto(BoundConditionalGoto node, IndentedTextWriter writer) {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(node.Label.Name);

        writer.WriteKeyword(node.Jump ? " if not " : " if ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer) {
        var unindent = writer.Indent > 0;
        if (unindent)
            writer.Indent--;
        
        writer.WritePunctuation(node.Label.Name);
        writer.WritePunctuation(":");

        if (unindent)
            writer.Indent++;

        writer.WriteLine();
    }

    private static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("if ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatements(node.IfStatement);

        if (node.ElseStatement != null) {
            writer.WriteKeyword("else");
            writer.WriteLine();
            writer.WriteNestedStatements(node.ElseStatement);
        }
    }

    private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("while ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
    }

    private static void WriteDoWhileStatement(BoundDoWhileStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("do ");
        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
        
        writer.WriteKeyword("while ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword("for ");
        writer.WriteIdentifier(node.Variable.Name);
        writer.WriteKeyword(" in ");
        node.Range.WriteTo(writer);

        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
    }
}