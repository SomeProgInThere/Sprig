using System.Collections.Immutable;

using Sprig.Codegen.Binding;
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen;

internal sealed class Evaluator(
    ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies, 
    BoundBlockStatement root, 
    Dictionary<VariableSymbol, object> globals
) {
        
    public object? Evaluate() {
        locals.Push([]);
        return EvaluateStatement(root);
    }

    private object? EvaluateStatement(BoundBlockStatement body) {
        var labelTable = new Dictionary<LabelSymbol, int>();

        for (var i = 0; i < body.Statements.Length; i++) {
            if (body.Statements[i] is BoundLabelStatement statement)
                labelTable.Add(statement.Label, i + 1);
        }

        var index = 0;
        while (index < body.Statements.Length) {
            var statement = body.Statements[index];
            switch (statement.Kind) {

                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)statement);
                    index++;
                    break;

                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)statement);
                    index++;
                    break;

                case BoundNodeKind.GotoStatement:
                    var gotoStatement = (BoundGotoStatement)statement;
                    index = labelTable[gotoStatement.Label];
                    break;

                case BoundNodeKind.ConditionalGotoStatement:
                    var conditionalGotoStatement = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);

                    if (condition && conditionalGotoStatement.Jump 
                        || !condition && !conditionalGotoStatement.Jump)
                        
                        index = labelTable[conditionalGotoStatement.Label];
                    else
                        index++;
                    break;

                case BoundNodeKind.LabelStatement:
                    index++;
                    break;

                case BoundNodeKind.ReturnStatement:
                    var returnStatment = (BoundReturnStatment)statement;
                    lastValue = returnStatment.Expression is null
                        ? null
                        : EvaluateExpression(returnStatment.Expression);

                    return lastValue;

                default:
                    throw new Exception($"Undefined statement: {statement.Kind}");
            }
        }

        return lastValue;
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclaration node) {
        var value = EvaluateExpression(node.Initializer);
        lastValue = value;
        AssignValue(node.Variable, value);
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
            BoundNodeKind.CastExpression        => EvaluateCastExpression((BoundCastExpression)node),
            
            _ => throw new Exception($"Undefined node: {node?.Kind}"),
        };
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression literal) => literal.Value;
    
    private object EvaluateVariableExpression(BoundVariableExpression node) {
        var variable = node.Variable 
            ?? throw new Exception($"Node variable not initialized");
        
        if (variable.Scope == VariableScope.Global)
            return globals[variable];
                
        else {
            var localVariables = locals.Peek();
            return localVariables[variable];
        }
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression node) {
        var value = EvaluateExpression(node.Expression);
        AssignValue(node.Variable, value);
        return value;
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression node) {
        var operand = EvaluateExpression(node.Operand);

        if (node.Type == TypeSymbol.Float) {
            return node.Operator.Kind switch {
                UnaryOperatorKind.Identity => (float)operand,
                UnaryOperatorKind.Negetion => -(float)operand,
                _ => throw new Exception($"Unexpected Unary operator: {node.Operator}"),
            };
        }
        
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

                if (node.Type == TypeSymbol.Float)
                    return (float)left + (float)right;

                return (int)left + (int)right;
            
            case BinaryOperatorKind.Substact:            
                if (node.Type == TypeSymbol.Float)
                    return (float)left - (float)right;

                return (int)left - (int)right;
            
            case BinaryOperatorKind.Multiply:
                if (node.Type == TypeSymbol.String) {
                    var str = (string)left;
                    for (var i = 1; i < (int)right; i++)
                        str += (string)left;

                    return str;
                }

                if (node.Type == TypeSymbol.Float)
                    return (float)left + (float)right;

                return (int)left * (int)right;

            case BinaryOperatorKind.Divide:                 
                if (node.Type == TypeSymbol.Float)
                    return (float)left / (float)right;

                return (int)left / (int)right;
            
            case BinaryOperatorKind.Modulus:            
                if (node.Type == TypeSymbol.Float)
                    return (float)left % (float)right;
 
                return (int)left % (int)right;
            
            case BinaryOperatorKind.Remainder: 
                return Math.DivRem((int)left, (int)right).Remainder;
            
            case BinaryOperatorKind.RaisePower:             
                if (node.Type == TypeSymbol.Float)
                    return (float)Math.Pow((float)left, (float)right);

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
             if (node.Type == TypeSymbol.Float)
                    return (float)left > (float)right;

                return (int)left > (int)right;
            
            case BinaryOperatorKind.GreaterThanEqualsTo:
             if (node.Type == TypeSymbol.Float)
                    return (float)left >= (float)right;

                return (int)left >= (int)right;
            
            case BinaryOperatorKind.LesserThan:
             if (node.Type == TypeSymbol.Float)
                    return (float)left < (float)right;

                return (int)left < (int)right;
            
            case BinaryOperatorKind.LesserThanEqualsTo:
             if (node.Type == TypeSymbol.Float)
                    return (float)left <= (float)right;

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

        else if (node.Function == BuiltinFunctions.Random) {
            var min = (int)EvaluateExpression(node.Arguments[0]);
            var max = (int)EvaluateExpression(node.Arguments[1]);
            
            random ??= new Random();
            return random.Next(min, max);
        }

        else {
            var stackframe = new Dictionary<VariableSymbol, object>();
            
            for (var i = 0; i < node.Arguments.Length; i++) {
                var parameter = node.Function.Parameters[i];
                var value = EvaluateExpression(node.Arguments[i]);
                stackframe.Add(parameter, value);
            }

            locals.Push(stackframe);
            var statement = functionBodies[node.Function];
            var result = EvaluateStatement(statement);
            locals.Pop();

            return result;
        }
    }

    private object EvaluateCastExpression(BoundCastExpression node) {
        var value = EvaluateExpression(node.Expression);
        
        if (node.Type == TypeSymbol.Bool)
            return Convert.ToBoolean(value);
        
        else if (node.Type == TypeSymbol.Int)
            return Convert.ToInt32(value);
        
        else if (node.Type == TypeSymbol.String)
            return Convert.ToString(value) ?? "";

        else if (node.Type == TypeSymbol.Float)
            return (float)Convert.ToDouble(value);
        else
            throw new Exception($"Undefined type: {node.Type}");
    }

    private void AssignValue(VariableSymbol variable, object value) {
        if (variable.Scope == VariableScope.Global)
            globals[variable] = value;

        else {
            var localVariables = locals.Peek();
            localVariables[variable] = value;
        }
    }

    private Random? random;
    private object? lastValue;
    private readonly Stack<Dictionary<VariableSymbol, object>> locals = [];
}

public sealed class EvaluationResult(ImmutableArray<DiagnosticMessage> diagnostics, object? result = null) {
    public ImmutableArray<DiagnosticMessage> Diagnostics { get; } = diagnostics;
    public object? Result { get; } = result;
}