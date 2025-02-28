using System.Collections.Immutable;

using Sprig.Code.Binding;
using Sprig.Code.Symbols;
using Sprig.Code.Syntax;

namespace Sprig.Code;

internal sealed class Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables) {
    
    public object? Evaluate() {
        var labelMap = new Dictionary<LabelSymbol, int>();

        for (var i = 0; i < root.Statements.Length; i++) {
            if (root.Statements[i] is BoundLableStatement statement)
                labelMap.Add(statement.Label, i + 1);
        }

        var index = 0;
        while (index < root.Statements.Length) {
            var statement = root.Statements[index];
            switch (statement.Kind) {
                case BoundNodeKind.VariableDeclarationStatement:
                    EvaluateVariableDeclaration((BoundVariableDeclarationStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.AssignOperationStatement:
                    EvaluateAssignOperationStatement((BoundAssignOperationStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.GotoStatement:
                    var gotoStatement = (BoundGotoStatement)statement;
                    index = labelMap[gotoStatement.Label];
                    break;

                case BoundNodeKind.ConditionalGotoStatement:
                    var conditionalGotoStatement = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);
                    
                    if (condition && conditionalGotoStatement.Jump || !condition && !conditionalGotoStatement.Jump)
                        index = labelMap[conditionalGotoStatement.Label];
                    else 
                        index++;
                    break;
                
                case BoundNodeKind.LabelStatement:
                    index++;
                    break;

                default:
                    throw new Exception($"Undefined statement: {statement.Kind}");
            }
        } 

        return lastValue;
    }

    private void EvaluateAssignOperationStatement(BoundAssignOperationStatement node) {
        var value = (int)EvaluateExpression(node.Expression);

        if (node.Variable == null) 
            return;
        
        var kind = node.AssignOperatorToken.Kind;
        variables[node.Variable] = kind switch {
            SyntaxKind.PlusEqualsToken          => (int)variables[node.Variable] + value,
            SyntaxKind.MinusEqualsToken         => (int)variables[node.Variable] - value,
            SyntaxKind.StarEqualsToken          => (int)variables[node.Variable] * value,
            SyntaxKind.SlashEqualsToken         => (int)variables[node.Variable] / value,
            SyntaxKind.PercentEqualsToken       => (int)variables[node.Variable] % value,
            SyntaxKind.AmpersandEqualsToken     => (int)variables[node.Variable] & value,
            SyntaxKind.PipeEqualsToken          => (int)variables[node.Variable] | value,
            SyntaxKind.CircumflexEqualsToken    => (int)variables[node.Variable] ^ value,
                
            _ => throw new Exception($"Undefined assignment operator: {kind}"),
        };

        lastValue = variables[node.Variable];
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclarationStatement node) {
        var value = EvaluateExpression(node.Initializer);
        variables[node.Variable] = value;
        lastValue = value;
    }
    
    private void EvaluateExpressionStatement(BoundExpressionStatement node) => lastValue = EvaluateExpression(node.Expression);
    
    private object EvaluateExpression(BoundExpression? node) {
        return node?.Kind switch {
            BoundNodeKind.LiteralExpression     => EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.VariableExpression    => EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.AssignmentExpression  => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.UnaryExpression       => EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression      => EvaluateBinaryExpression((BoundBinaryExpression)node),
            BoundNodeKind.RangeExpression       => EvaluateRangeExpression((BoundRangeExpression)node),
            BoundNodeKind.CallExpression        => EvaluateCallExpression((BoundCallExpression)node),
            
            _ => throw new Exception($"Undefined node: {node?.Kind}"),
        };
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression literal) => literal.Value;
    
    private object EvaluateVariableExpression(BoundVariableExpression node) {
        if (node.Variable is null)
            throw new Exception($"Variable: {nameof(node.Variable)} is not initialized");
        
        return variables[node.Variable];
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression node) {
        var value = EvaluateExpression(node.Expression);
        variables[node.Variable] = value;
        return value;
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression node) {
        var operand = EvaluateExpression(node.Operand);

        switch (node.Operator.Kind) {
            case UnaryOperatorKind.Identity:
                return (int)operand;
                
            case UnaryOperatorKind.Negetion:
                return -(int)operand;

            case UnaryOperatorKind.BitwiseNot:
                return ~(int)operand;

            case UnaryOperatorKind.LogicalNot:
                return !(bool)operand;

            case UnaryOperatorKind.PostIncrement: {
                var value = (int)operand;
                value++;
                return value;
            }

            case UnaryOperatorKind.PostDecrement: {
                var value = (int)operand;
                value--;
                return value;
            }
                
            case UnaryOperatorKind.PreIncrement: {
                var value = (int)operand;
                ++value;
                return value;
            }
            
            case UnaryOperatorKind.PreDecrement: {
                var value = (int)operand;
                --value;
                return value;
            }
            
            default:
                throw new Exception($"Unexpected Unary operator: {node.Operator}");
        }
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression node) {
        var left = EvaluateExpression(node.Left);
        var right = EvaluateExpression(node.Right);

        switch (node.Operator.Kind) {
            case BinaryOperatorKind.Add: 
                if (node.Type == TypeSymbol.String)
                    return (string)left + (string)right;

                return (int)left + (int)right;
            
            case BinaryOperatorKind.Substact: 
                return (int)left - (int)right;
            
            case BinaryOperatorKind.Multiply: 
                if (node.Type == TypeSymbol.String) {
                    var str = (string)left;
                    for (var i = 1; i < (int)right; i++)
                        str += (string)left;

                    return str;
                }

                return (int)left * (int)right;

            case BinaryOperatorKind.Divide: 
                return (int)left / (int)right;
            
            case BinaryOperatorKind.Modulus: 
                return (int)left % (int)right;
            
            case BinaryOperatorKind.Remainder: 
                return Math.DivRem((int)left, (int)right).Remainder;
            
            case BinaryOperatorKind.RaisePower: 
                return (int)Math.Pow((int)left, (int)right);
             
            case BinaryOperatorKind.LogicalAnd: 
                return (bool)left && (bool)right;
            
            case BinaryOperatorKind.LogicalOr: 
                return (bool)left || (bool)right;

            case BinaryOperatorKind.BitwiseAnd: 
                return (int)left & (int)right;
            
            case BinaryOperatorKind.BitwiseOr: 
                return (int)left | (int)right;
            
            case BinaryOperatorKind.BitwiseXor: 
                return (int)left ^ (int)right;
            
            case BinaryOperatorKind.BitshiftLeft: 
                return (int)left >> (int)right;
            
            case BinaryOperatorKind.BitshiftRight: 
                return (int)left << (int)right;

            case BinaryOperatorKind.GreaterThan: 
                return (int)left > (int)right;
            
            case BinaryOperatorKind.GreaterThanEqualsTo: 
                return (int)left >= (int)right;
            
            case BinaryOperatorKind.LesserThan:
                return (int)left < (int)right;
            
            case BinaryOperatorKind.LesserThanEqualsTo: 
                return (int)left <= (int)right;

            case BinaryOperatorKind.Equals: 
                return Equals(left, right);
            
            case BinaryOperatorKind.NotEquals: 
                return !Equals(left, right);

            default: 
                throw new Exception($"Unexpected Binary operator: {node.Operator}");
        };
    }

    private object EvaluateRangeExpression(BoundRangeExpression node) {
        var lower = EvaluateExpression(node.Lower);
        var upper = EvaluateExpression(node.Upper);
        return (lower, upper);
    }

    private object EvaluateCallExpression(BoundCallExpression node) {
        if (node.Function == BuiltinFunctions.Input) {
            return Console.ReadLine() ?? "";
        }

        else if (node.Function == BuiltinFunctions.Print) {
            var message = (string)EvaluateExpression(node.Arguments[0]);
            Console.WriteLine(message);
            return "";
        }

        else {
            throw new Exception($"Unexpected function: {node.Function}");
        }
    }

    private object? lastValue;
}

public sealed class EvaluationResult(ImmutableArray<DiagnosticMessage> diagnostics, object? result = null) {
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public object? Result { get; } = result;
}