
using Sprig.Codegen.Symbols;

namespace Sprig.Codegen.IR;

internal static class ConstantFolding {

    public static IR_Constant? ComputeConstant(IR_UnaryOperator op, IR_Expression operand) {
        if (operand.ConstantValue != null) {
            var value = operand.ConstantValue.Value;

            switch (op.Kind) {
                case UnaryOperator.Identity:
                    if (operand.Type == TypeSymbol.Double)
                        return new IR_Constant((double)value);

                    return new IR_Constant((int)value);

                case UnaryOperator.Negetion:
                    if (operand.Type == TypeSymbol.Double)
                        return new IR_Constant(-(double)value);

                    return new IR_Constant(-(int)value);

                case UnaryOperator.BitwiseNot:
                    return new IR_Constant(~(int)value);

                case UnaryOperator.LogicalNot:
                    return new IR_Constant(!(bool)value);

                default:
                    throw new Exception($"Unexpected Unary operator: {op.Kind}");
            }   
        }

        return null;
    }

    public static IR_Constant? ComputeConstant(IR_Expression left, IR_BinaryOperator op, IR_Expression right) {
        var leftConstant = left.ConstantValue;
        var rightConstant = right.ConstantValue;

        // Special cases for Logical AND and OR
        if (op.Kind == BinaryOperator.LogicalAnd) {

            if (leftConstant != null && !(bool)leftConstant.Value 
                || rightConstant != null && !(bool)rightConstant.Value)
                return new IR_Constant(false);
        }

        if (op.Kind == BinaryOperator.LogicalOr) {

            if (leftConstant != null && (bool)leftConstant.Value 
                || rightConstant != null && (bool)rightConstant.Value)
                return new IR_Constant(true);    
        }

        if (leftConstant is null || rightConstant is null)
            return null;
    
        var leftValue = leftConstant.Value;
        var rightValue = rightConstant.Value;

        switch (op.Kind) {
            case BinaryOperator.Add: 
                if (left.Type == TypeSymbol.String)
                    return new IR_Constant((string)leftValue + (string)rightValue);

                if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue + (double)rightValue);

                return new IR_Constant((int)leftValue + (int)rightValue);
            
            case BinaryOperator.Substact:            
                if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue - (double)rightValue);

                return new IR_Constant((int)leftValue - (int)rightValue);
            
            case BinaryOperator.Multiply:
                if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue + (double)rightValue);

                return new IR_Constant((int)leftValue * (int)rightValue);

            case BinaryOperator.Divide:                 
                if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue / (double)rightValue);

                return new IR_Constant((int)leftValue / (int)rightValue);
                        
            case BinaryOperator.Remainder:
                if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue % (double)rightValue);
            
                return new IR_Constant((int)leftValue % (int)rightValue);
             
            case BinaryOperator.LogicalAnd: 
                return new IR_Constant((bool)leftValue && (bool)rightValue);
            
            case BinaryOperator.LogicalOr: 
                return new IR_Constant((bool)leftValue || (bool)rightValue);

            case BinaryOperator.BitwiseAnd: 
                return new IR_Constant((int)leftValue & (int)rightValue);
            
            case BinaryOperator.BitwiseOr: 
                return new IR_Constant((int)leftValue | (int)rightValue);
            
            case BinaryOperator.BitwiseXor: 
                return new IR_Constant((int)leftValue ^ (int)rightValue);
            
            case BinaryOperator.BitshiftLeft: 
                return new IR_Constant((int)leftValue >> (int)rightValue);
            
            case BinaryOperator.BitshiftRight: 
                return new IR_Constant((int)leftValue << (int)rightValue);

            case BinaryOperator.GreaterThan:
             if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue > (double)rightValue);

                return new IR_Constant((int)leftValue > (int)rightValue);
            
            case BinaryOperator.GreaterThanEqualsTo:
             if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue >= (double)rightValue);

                return new IR_Constant((int)leftValue >= (int)rightValue);
            
            case BinaryOperator.LesserThan:
             if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue < (double)rightValue);

                return new IR_Constant((int)leftValue < (int)rightValue);
            
            case BinaryOperator.LesserThanEqualsTo:
             if (left.Type == TypeSymbol.Double)
                    return new IR_Constant((double)leftValue <= (double)rightValue);

                return new IR_Constant((int)leftValue <= (int)rightValue);

            case BinaryOperator.Equals: 
                return new IR_Constant(Equals(leftValue, rightValue));
            
            case BinaryOperator.NotEquals: 
                return new IR_Constant(!Equals(leftValue, rightValue));

            default: 
                throw new Exception($"Unexpected Binary Operator: {left.Kind}");
        }
    }
}

public sealed class IR_Constant(object value) {
    public object Value { get; } = value;
}