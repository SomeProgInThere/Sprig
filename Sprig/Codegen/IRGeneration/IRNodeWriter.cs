using System.CodeDom.Compiler;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;
using Sprig.IO;

namespace Sprig.Codegen.IRGeneration;

internal static class IRNodeWriter {

    public static void WriteTo(this IRNode node, TextWriter writer) {
        if (writer is IndentedTextWriter indentedWriter)
            WriteTo(node, indentedWriter);
        else
            WriteTo(node, new IndentedTextWriter(writer));     
    }

    public static void WriteTo(this IRNode node, IndentedTextWriter writer) {
        switch (node.Kind) {
            case IRNodeKind.LiteralExpression: 
                WriteLiteralExpression((IRLiteralExpression)node, writer);
                break;
            
            case IRNodeKind.VariableExpression: 
                WriteVariableExpression((IRVariableExpression)node, writer);
                break;
                
            case IRNodeKind.AssignmentExpression: 
                WriteAssignmentExpression((IRAssignmentExpression)node, writer);
                break;
                
            case IRNodeKind.UnaryExpression: 
                WriteUnaryExpression((IRUnaryExpression)node, writer);
                break;
                
            case IRNodeKind.BinaryExpression: 
                WriteBinaryExpression((IRBinaryExpression)node, writer);
                break;
                
            case IRNodeKind.RangeExpression: 
                WriteRangeExpression((IRRangeExpression)node, writer);
                break;
                
            case IRNodeKind.CallExpression: 
                WriteCallExpression((IRCallExpression)node, writer);
                break;
                
            case IRNodeKind.CastExpression: 
                WriteCastExpression((IRCastExpression)node, writer);
                break;
                
            case IRNodeKind.ErrorExpression: 
                writer.WriteIdentifier("?");
                break;
                
            case IRNodeKind.BlockStatement: 
                WriteBlockStatement((IRBlockStatement)node, writer);
                break;
                
            case IRNodeKind.VariableDeclaration: 
                WriteVariableDeclaration((IRVariableDeclaration)node, writer);
            break;

            case IRNodeKind.ExpressionStatement: 
                WriteExpressionStatement((IRExpressionStatement)node, writer);
                break;
                
            case IRNodeKind.GotoStatement: 
                WriteGotoStatement((IRGotoStatement)node, writer);
                break;
                
            case IRNodeKind.ConditionalGotoStatement: 
                WriteConditionalGoto((IRConditionalGotoStatement)node, writer);
                break;
                
            case IRNodeKind.LabelStatement: 
                WriteLabelStatement((IRLabelStatement)node, writer);
                break;
                
            case IRNodeKind.ReturnStatement:
                WriteReturnStatement((IRReturnStatment)node, writer);
                break;

            case IRNodeKind.IfStatement: 
                WriteIfStatement((IRIfStatement)node, writer);
                break;
            
            case IRNodeKind.WhileStatement: 
                WriteWhileStatement((IRWhileStatement)node, writer);
                break;
                
            case IRNodeKind.DoWhileStatement: 
                WriteDoWhileStatement((IRDoWhileStatement)node, writer);
                break;
                
            case IRNodeKind.ForStatement: 
                WriteForStatement((IRForStatement)node, writer);
                break;

            default:
                throw new Exception($"Unexpected node: {node.Kind}");
        }
    }

    private static void WriteNestedExpressions(
        this IndentedTextWriter writer, 
        int parentPrecendence, 
        IRExpression node
    ) {
        if (node is IRUnaryExpression unary)
            writer.WriteNestedExpressions(
                parentPrecendence, 
                unary.Operator.SyntaxKind.UnaryOperatorPrecedence(), 
                unary
            );
        
        else if (node is IRBinaryExpression binary)
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
        IRExpression node
    ) {
        var needsParenthesis = parentPrecendence >= currentPrecedence;
        if (needsParenthesis)
            writer.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        
        node.WriteTo(writer);

        if (needsParenthesis)
            writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken);
    }

    private static void WriteLiteralExpression(IRLiteralExpression node, IndentedTextWriter writer) {
        var value = node.Value.ToString() ?? "?";
        
        if (node.Type == TypeSymbol.Bool)
            writer.WriteIdentifier(value);

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

    private static void WriteVariableExpression(IRVariableExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Variable.Name);
    }

    private static void WriteAssignmentExpression(IRAssignmentExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Variable.Name);

        writer.WriteSpace();
        writer.WritePunctuation(SyntaxKind.EqualsToken);
        writer.WriteSpace();

        node.Expression.WriteTo(writer);
    }

    private static void WriteUnaryExpression(IRUnaryExpression node, IndentedTextWriter writer) {
        var kind = node.Operator.SyntaxKind;
        var precedence = kind.UnaryOperatorPrecedence();
        
        writer.WritePunctuation(kind);
        writer.WriteNestedExpressions(precedence, node.Operand);
    }

    private static void WriteBinaryExpression(IRBinaryExpression node, IndentedTextWriter writer) {
        var kind = node.Operator.SyntaxKind;
        var precedence = kind.BinaryOperatorPrecedence();

        writer.WriteNestedExpressions(precedence, node.Left);        
        writer.WriteSpace();
        writer.WritePunctuation(kind);
        writer.WriteSpace();
        writer.WriteNestedExpressions(precedence, node.Right);        
    }

    private static void WriteRangeExpression(IRRangeExpression node, IndentedTextWriter writer) {}

    private static void WriteCallExpression(IRCallExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Function.Name);
        writer.WritePunctuation(SyntaxKind.OpenParenthesisToken);

        var isFirst = true;
        foreach (var argument in node.Arguments) {
            if (isFirst)
                isFirst = false;
            else {
                writer.WritePunctuation(SyntaxKind.CommaToken);
                writer.WriteSpace();
            }

            argument.WriteTo(writer);
        }

        writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken);
    }

    private static void WriteCastExpression(IRCastExpression node, IndentedTextWriter writer) {
        writer.WriteIdentifier(node.Type.Name);
        writer.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        node.Expression.WriteTo(writer);
        writer.WritePunctuation(SyntaxKind.ClosedParenthesisToken);
    }

    private static void WriteNestedStatements(this IndentedTextWriter writer, IRStatement node) {
        var needsIndent = node is not IRBlockStatement;
        if (needsIndent)
            writer.Indent++;

        node.WriteTo(writer);
        if (needsIndent)
            writer.Indent--;
    }

    private static void WriteBlockStatement(IRBlockStatement node, IndentedTextWriter writer) {
        writer.WritePunctuation(SyntaxKind.OpenBraceToken);
        writer.WriteLine();
        writer.Indent++;

        foreach (var statement in node.Statements)
            statement.WriteTo(writer);

        writer.Indent--;
        writer.WritePunctuation(SyntaxKind.ClosedBraceToken);
        writer.WriteLine();
    }

    private static void WriteVariableDeclaration(IRVariableDeclaration node, IndentedTextWriter writer) {
        writer.WriteKeyword(node.Variable.Mutable ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword);
        writer.WriteSpace();
        writer.WriteIdentifier(node.Variable.Name);

        writer.WriteSpace();
        writer.WritePunctuation(SyntaxKind.EqualsToken);
        writer.WriteSpace();

        node.Initializer.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteExpressionStatement(IRExpressionStatement node, IndentedTextWriter writer) {
        node.Expression.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteGotoStatement(IRGotoStatement node, IndentedTextWriter writer) {
        writer.WriteInfo("goto");
        writer.WriteSpace();
        writer.WriteIdentifier(node.Label.Name);
        writer.WriteLine();
    }

    private static void WriteConditionalGoto(IRConditionalGotoStatement node, IndentedTextWriter writer) {
        writer.WriteInfo("goto");
        writer.WriteSpace();
        writer.WriteIdentifier(node.Label.Name);

        writer.WriteSpace();
        writer.WriteInfo(node.Jump ? "if" : "if not");
        writer.WriteSpace();
        
        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteLabelStatement(IRLabelStatement node, IndentedTextWriter writer) {
        var unindent = writer.Indent > 0;
        if (unindent)
            writer.Indent--;
        
        writer.WriteIdentifier(node.Label.Name);
        writer.WritePunctuation(SyntaxKind.ColonToken);

        if (unindent)
            writer.Indent++;

        writer.WriteLine();
    }

    private static void WriteReturnStatement(IRReturnStatment node, IndentedTextWriter writer) {
        writer.WriteKeyword(SyntaxKind.ReturnKeyword);
        if (node.Expression != null) {
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        writer.WriteLine();
    }

    private static void WriteIfStatement(IRIfStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword(SyntaxKind.IfKeyword);
        writer.WriteSpace();

        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatements(node.IfStatement);

        if (node.ElseStatement != null) {
            writer.WriteKeyword(SyntaxKind.ElseKeyword);
            writer.WriteLine();
            writer.WriteNestedStatements(node.ElseStatement);
        }
    }

    private static void WriteWhileStatement(IRWhileStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword(SyntaxKind.WhileKeyword);
        writer.WriteSpace();

        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
    }

    private static void WriteDoWhileStatement(IRDoWhileStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword(SyntaxKind.DoKeyword);
        writer.WriteSpace();
        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
        
        writer.WriteKeyword(SyntaxKind.WhileKeyword);
        writer.WriteSpace();

        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteForStatement(IRForStatement node, IndentedTextWriter writer) {
        writer.WriteKeyword(SyntaxKind.ForKeyword);
        writer.WriteSpace();
        writer.WriteIdentifier(node.Variable.Name);

        writer.WriteSpace();
        writer.WriteKeyword(SyntaxKind.InKeyword);
        writer.WriteSpace();

        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
    }
}