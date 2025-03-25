using System.CodeDom.Compiler;
using Sprig.Codegen.Symbols;
using Sprig.Codegen.Syntax;
using Sprig.IO;

namespace Sprig.Codegen.IR;

internal static class IRNodeWriter {

    public static void WriteTo(this IR_Node node, TextWriter writer) {
        if (writer is IndentedTextWriter indentedWriter)
            WriteTo(node, indentedWriter);
        else
            WriteTo(node, new IndentedTextWriter(writer));     
    }

    public static void WriteTo(this IR_Node node, IndentedTextWriter writer) {
        switch (node.Kind) {
            case IR_NodeKind.LiteralExpression: 
                WriteLiteralExpression((IR_LiteralExpression)node, writer);
                break;
            
            case IR_NodeKind.VariableExpression: 
                WriteVariableExpression((IR_VariableExpression)node, writer);
                break;
                
            case IR_NodeKind.AssignmentExpression: 
                WriteAssignmentExpression((IR_AssignmentExpression)node, writer);
                break;
                
            case IR_NodeKind.UnaryExpression: 
                WriteUnaryExpression((IR_UnaryExpression)node, writer);
                break;
                
            case IR_NodeKind.BinaryExpression: 
                WriteBinaryExpression((IR_BinaryExpression)node, writer);
                break;
                
            case IR_NodeKind.CallExpression: 
                WriteCallExpression((IR_CallExpression)node, writer);
                break;
                
            case IR_NodeKind.CastExpression: 
                WriteCastExpression((IR_CastExpression)node, writer);
                break;
                
            case IR_NodeKind.ErrorExpression: 
                writer.Write("?");
                break;
                
            case IR_NodeKind.BlockStatement: 
                WriteBlockStatement((IR_BlockStatement)node, writer);
                break;

            case IR_NodeKind.NopStatement: 
                WriteNopStatement(writer);
                break;
                
            case IR_NodeKind.VariableDeclaration: 
                WriteVariableDeclaration((IR_VariableDeclaration)node, writer);
            break;

            case IR_NodeKind.ExpressionStatement: 
                WriteExpressionStatement((IR_ExpressionStatement)node, writer);
                break;
                
            case IR_NodeKind.GotoStatement: 
                WriteGotoStatement((IR_GotoStatement)node, writer);
                break;
                
            case IR_NodeKind.ConditionalGotoStatement: 
                WriteConditionalGoto((IR_ConditionalGotoStatement)node, writer);
                break;
                
            case IR_NodeKind.LabelStatement: 
                WriteLabelStatement((IR_LabelStatement)node, writer);
                break;
                
            case IR_NodeKind.ReturnStatement:
                WriteReturnStatement((IR_ReturnStatment)node, writer);
                break;

            case IR_NodeKind.IfStatement: 
                WriteIfStatement((IR_IfStatement)node, writer);
                break;
            
            case IR_NodeKind.WhileStatement: 
                WriteWhileStatement((IR_WhileStatement)node, writer);
                break;
                
            case IR_NodeKind.DoWhileStatement: 
                WriteDoWhileStatement((IR_DoWhileStatement)node, writer);
                break;
                
            case IR_NodeKind.ForStatement: 
                WriteForStatement((IR_ForStatement)node, writer);
                break;

            default:
                throw new Exception($"Unexpected node: {node.Kind}");
        }
    }

    private static void WriteNestedExpressions(
        this IndentedTextWriter writer, 
        int parentPrecendence, 
        IR_Expression node
    ) {
        if (node is IR_UnaryExpression unary)
            writer.WriteNestedExpressions(
                parentPrecendence, 
                unary.Operator.SyntaxKind.UnaryOperatorPrecedence(), 
                unary
            );
        
        else if (node is IR_BinaryExpression binary)
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
        IR_Expression node
    ) {
        var needsParenthesis = parentPrecendence >= currentPrecedence;
        if (needsParenthesis)
            writer.WriteToken(SyntaxKind.OpenParenthesisToken);
        
        node.WriteTo(writer);

        if (needsParenthesis)
            writer.WriteToken(SyntaxKind.ClosedParenthesisToken);
    }

    private static void WriteLiteralExpression(IR_LiteralExpression node, IndentedTextWriter writer) {
        var value = node.Value.ToString() ?? "?";
        
        if (node.Type == TypeSymbol.Boolean)
            writer.Write(value);

        else if (node.Type == TypeSymbol.Int32)
            writer.Write(value);
            
        else if (node.Type == TypeSymbol.Double)
            writer.Write(value);

        else if (node.Type == TypeSymbol.String) {
            value = "\"" + value.Replace("\"", "\"\"") + "\"";
            writer.Write(value);
        }

        else throw new Exception($"Unexpected type: {node.Type}");
    }

    private static void WriteVariableExpression(IR_VariableExpression node, IndentedTextWriter writer) {
        writer.Write(node.Variable.Name);
    }

    private static void WriteAssignmentExpression(IR_AssignmentExpression node, IndentedTextWriter writer) {
        writer.Write(node.Variable.Name);

        writer.WriteSpace();
        writer.WriteToken(SyntaxKind.EqualsToken);
        writer.WriteSpace();

        node.Expression.WriteTo(writer);
    }

    private static void WriteUnaryExpression(IR_UnaryExpression node, IndentedTextWriter writer) {
        var kind = node.Operator.SyntaxKind;
        var precedence = kind.UnaryOperatorPrecedence();
        
        writer.WriteToken(kind);
        writer.WriteNestedExpressions(precedence, node.Operand);
    }

    private static void WriteBinaryExpression(IR_BinaryExpression node, IndentedTextWriter writer) {
        var kind = node.Operator.SyntaxKind;
        var precedence = kind.BinaryOperatorPrecedence();

        writer.WriteNestedExpressions(precedence, node.Left);        
        writer.WriteSpace();
        writer.WriteToken(kind);
        writer.WriteSpace();
        writer.WriteNestedExpressions(precedence, node.Right);        
    }

    private static void WriteCallExpression(IR_CallExpression node, IndentedTextWriter writer) {
        writer.Write(node.Function.Name);
        writer.WriteToken(SyntaxKind.OpenParenthesisToken);

        var isFirst = true;
        foreach (var argument in node.Arguments) {
            if (isFirst)
                isFirst = false;
            else {
                writer.WriteToken(SyntaxKind.CommaToken);
                writer.WriteSpace();
            }

            argument.WriteTo(writer);
        }

        writer.WriteToken(SyntaxKind.ClosedParenthesisToken);
    }

    private static void WriteCastExpression(IR_CastExpression node, IndentedTextWriter writer) {
        writer.Write(node.Type.Name);
        writer.WriteToken(SyntaxKind.OpenParenthesisToken);
        node.Expression.WriteTo(writer);
        writer.WriteToken(SyntaxKind.ClosedParenthesisToken);
    }

    private static void WriteNestedStatements(this IndentedTextWriter writer, IR_Statement node) {
        var needsIndent = node is not IR_BlockStatement;
        if (needsIndent)
            writer.Indent++;

        node.WriteTo(writer);
        if (needsIndent)
            writer.Indent--;
    }

    private static void WriteBlockStatement(IR_BlockStatement node, IndentedTextWriter writer) {
        writer.WriteSpace();
        writer.WriteToken(SyntaxKind.OpenBraceToken);
        writer.WriteLine();
        writer.Indent++;

        foreach (var statement in node.Statements)
            statement.WriteTo(writer);

        writer.Indent--;
        writer.WriteToken(SyntaxKind.ClosedBraceToken);
        writer.WriteLine();
    }

    private static void WriteNopStatement(IndentedTextWriter writer) {
        writer.WriteText("nop");
        writer.WriteLine();
    }

    private static void WriteVariableDeclaration(IR_VariableDeclaration node, IndentedTextWriter writer) {
        writer.WriteToken(node.Variable.Mutable ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword);
        writer.WriteSpace();
        writer.Write(node.Variable.Name);

        writer.WriteSpace();
        writer.WriteToken(SyntaxKind.EqualsToken);
        writer.WriteSpace();

        node.Initializer.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteExpressionStatement(IR_ExpressionStatement node, IndentedTextWriter writer) {
        node.Expression.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteGotoStatement(IR_GotoStatement node, IndentedTextWriter writer) {
        writer.WriteText("goto");
        writer.WriteSpace();
        writer.Write(node.Label.Name);
        writer.WriteLine();
    }

    private static void WriteConditionalGoto(IR_ConditionalGotoStatement node, IndentedTextWriter writer) {
        writer.Write(node.Jump ? "if " : "if !");
        writer.WriteToken(SyntaxKind.OpenParenthesisToken);
        node.Condition.WriteTo(writer);
        writer.WriteToken(SyntaxKind.ClosedParenthesisToken);

        writer.WriteLine();
        writer.Write("\t");
        writer.WriteText("goto");
        writer.WriteSpace();
        writer.Write(node.Label.Name);
        
        writer.WriteLine();
    }

    private static void WriteLabelStatement(IR_LabelStatement node, IndentedTextWriter writer) {
        var unindent = writer.Indent > 0;
        if (unindent)
            writer.Indent--;
        
        writer.Write(node.Label.Name);
        writer.WriteToken(SyntaxKind.ColonToken);

        if (unindent)
            writer.Indent++;

        writer.WriteLine();
    }

    private static void WriteReturnStatement(IR_ReturnStatment node, IndentedTextWriter writer) {
        writer.WriteToken(SyntaxKind.ReturnKeyword);
        if (node.Expression != null) {
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        writer.WriteLine();
    }

    private static void WriteIfStatement(IR_IfStatement node, IndentedTextWriter writer) {
        writer.WriteToken(SyntaxKind.IfKeyword);
        writer.WriteSpace();

        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatements(node.IfStatement);

        if (node.ElseStatement != null) {
            writer.WriteToken(SyntaxKind.ElseKeyword);
            writer.WriteLine();
            writer.WriteNestedStatements(node.ElseStatement);
        }
    }

    private static void WriteWhileStatement(IR_WhileStatement node, IndentedTextWriter writer) {
        writer.WriteToken(SyntaxKind.WhileKeyword);
        writer.WriteSpace();

        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
    }

    private static void WriteDoWhileStatement(IR_DoWhileStatement node, IndentedTextWriter writer) {
        writer.WriteToken(SyntaxKind.DoKeyword);
        writer.WriteSpace();
        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
        
        writer.WriteToken(SyntaxKind.WhileKeyword);
        writer.WriteSpace();

        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteForStatement(IR_ForStatement node, IndentedTextWriter writer) {
        writer.WriteToken(SyntaxKind.ForKeyword);
        writer.WriteSpace();
        writer.Write(node.Variable.Name);
        writer.WriteSpace();
        writer.WriteToken(SyntaxKind.InKeyword);
        writer.WriteSpace();

        node.LowerBound.WriteTo(writer);
        writer.WriteSpace();
        writer.WriteToken(SyntaxKind.DoubleDotToken);
        writer.WriteSpace();
        node.UpperBound.WriteTo(writer);

        writer.WriteLine();
        writer.WriteNestedStatements(node.Body);
    }
}