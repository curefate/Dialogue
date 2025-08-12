using System.Text;

namespace DS.Core
{
    public class Expression
    {
        private DSExpressionNode _root;

        public DSExpressionNode Root => _root;

        internal Expression(DSExpressionNode root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root), "Root node cannot be null.");
        }

        public object Evaluate(Runtime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime), "Runtime Environment cannot be null.");
            }
            return _root.Evaluate(runtime);
        }

        public override string ToString()
        {
            return _root.ToString();
        }

        public static Expression Constant(object value)
        {
            return new Expression(new ConstantNode(value));
        }

        public static Expression Variable(string variableName)
        {
            if (string.IsNullOrWhiteSpace(variableName))
            {
                throw new ArgumentException("Variable name cannot be null or empty.", nameof(variableName));
            }
            return new Expression(new VariableNode(variableName));
        }

        public static Expression Call(string functionName, params Expression[] arguments)
        {
            if (string.IsNullOrWhiteSpace(functionName))
            {
                throw new ArgumentException("Function name cannot be null or empty.", nameof(functionName));
            }
            // *Disable null arguments to prevent issues with null nodes
            var argNodes = arguments?.Select(arg => arg?._root).Where(node => node != null).Cast<DSExpressionNode>().ToList() ?? new List<DSExpressionNode>();
            return new Expression(new CallNode(functionName, argNodes));
        }

        public static Expression FString(List<string> fragments, List<Expression> embed)
        {
            if (fragments == null) throw new ArgumentNullException(nameof(fragments), "Fragments cannot be null.");
            if (embed == null) throw new ArgumentNullException(nameof(embed), "Embed expressions cannot be null.");
            // *Disable null arguments to prevent issues with null nodes
            var embedNodes = embed.Select(e => e?._root).Where(node => node != null).Cast<DSExpressionNode>().ToList();
            return new Expression(new FStringNode(fragments, embedNodes));
        }

        public static Expression Negate(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression), "Expression cannot be null.");
            return new Expression(new UnaryOperationNode(UnaryOperator.Negate, expression._root));
        }

        public static Expression Not(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression), "Expression cannot be null.");
            return new Expression(new UnaryOperationNode(UnaryOperator.Not, expression._root));
        }

        public static Expression Add(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Add, left._root, right._root));
        }

        public static Expression Subtract(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Subtract, left._root, right._root));
        }

        public static Expression Multiply(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Multiply, left._root, right._root));
        }

        public static Expression Divide(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Divide, left._root, right._root));
        }

        public static Expression Modulo(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Modulo, left._root, right._root));
        }

        public static Expression Equal(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Equal, left._root, right._root));
        }

        public static Expression NotEqual(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.NotEqual, left._root, right._root));
        }

        public static Expression LessThan(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.LessThan, left._root, right._root));
        }

        public static Expression GreaterThan(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.GreaterThan, left._root, right._root));
        }

        public static Expression LessThanOrEqual(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.LessThanOrEqual, left._root, right._root));
        }

        public static Expression GreaterThanOrEqual(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.GreaterThanOrEqual, left._root, right._root));
        }

        public static Expression AndAlso(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.AndAlso, left._root, right._root));
        }

        public static Expression OrElse(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.OrElse, left._root, right._root));
        }

        public static Expression Assign(Expression left, Expression right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left), "Left expression cannot be null.");
            if (right == null) throw new ArgumentNullException(nameof(right), "Right expression cannot be null.");
            return new Expression(new BinaryOperationNode(BinaryOperator.Assign, left._root, right._root));
        }
    }

    public abstract class DSExpressionNode
    {
        public Type Type { get; protected set; } = typeof(void);
        public abstract override string ToString();
        public abstract object Evaluate(Runtime runtime);
    }

    public class ConstantNode : DSExpressionNode
    {
        public object Value { get; private set; }

        internal ConstantNode(object value)
        {
            Value = value;
            var type = value.GetType();
            if (type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(string) || type == typeof(bool))
            {
                Type = type;
            }
            else
            {
                throw new ArgumentException($"Unsupported constant type: {type}");
            }
        }

        public override string ToString()
        {
            return Type switch
            {
                Type t when t == typeof(int) => $"{Value}",
                Type t when t == typeof(float) => $"{Value}f",
                Type t when t == typeof(double) => $"{Value}d",
                Type t when t == typeof(string) => $"\"{Value}\"",
                Type t when t == typeof(bool) => (bool)Value ? "true" : "false",
                _ => throw new InvalidOperationException($"Unsupported constant type: {Type}")
            };
        }

        public override object Evaluate(Runtime? runtime = null)
        {
            return Value;
        }
    }

    public class VariableNode : DSExpressionNode
    {
        public string VariableName { get; private set; }

        internal VariableNode(string variableName)
        {
            VariableName = variableName;
        }

        public override string ToString()
        {
            return $"${VariableName}";
        }

        public override object Evaluate(Runtime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime), "Runtime Environment cannot be null.");
            }

            var value = runtime.Variables.Get(VariableName) ?? throw new InvalidOperationException($"Variable '{VariableName}' is not defined.");
            Type = value.Type;
            return value.Value;
        }
    }

    public class CallNode : DSExpressionNode
    {
        public string FunctionName { get; private set; }
        public List<DSExpressionNode> Arguments { get; private set; }

        internal CallNode(string functionName, List<DSExpressionNode> arguments)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName), "Function name cannot be null.");
            Arguments = arguments;
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(arg => arg.ToString()));
            return $"{FunctionName}({args})";
        }

        public override object Evaluate(Runtime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime), "Runtime Environment cannot be null.");
            }
            var argValues = new List<object>();
            foreach (var arg in Arguments)
            {
                argValues.Add(arg.Evaluate(runtime));
            }
            Type = runtime.Functions.GetDelegate(FunctionName)?.Method.ReturnType ?? typeof(void);
            var result = runtime.Functions.Invoke(FunctionName, [.. argValues]);
            return result ?? null!;
        }
    }

    public class FStringNode : DSExpressionNode
    {
        public static readonly string EmbedSign = "{_0_}";

        public List<string> Fragments { get; private set; }
        public List<DSExpressionNode> EmbedExpr { get; private set; }

        internal FStringNode(List<string> fragments, List<DSExpressionNode> embedExprs)
        {
            Fragments = fragments ?? throw new ArgumentNullException(nameof(fragments), "Fragments cannot be null.");
            EmbedExpr = embedExprs ?? throw new ArgumentNullException(nameof(embedExprs), "Embed cannot be null.");
        }

        public override string ToString()
        {
            var embedStr = string.Join("", EmbedExpr.Select(e => e.ToString()));
            return $"\"{string.Join("", Fragments)}{embedStr}\"";
        }

        public override object Evaluate(Runtime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime), "Runtime Environment cannot be null.");
            }

            var finalStr = new StringBuilder();
            int embedIndex = 0;
            foreach (var fragment in Fragments)
            {
                if (fragment == EmbedSign)
                {
                    if (embedIndex < EmbedExpr.Count)
                    {
                        var embedValue = EmbedExpr[embedIndex].Evaluate(runtime);
                        if (embedValue is string embedString)
                        {
                            finalStr.Append(embedString);
                        }
                        else if (Convert.ToString(embedValue) is string embedConverted)
                        {
                            finalStr.Append(embedConverted);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Embedded expression must can be evaluated to a string, but got {embedValue.GetType()}.");
                        }
                        embedIndex++;
                    }
                    else
                    {
                        throw new InvalidOperationException("Not enough embedded expressions provided for FString.");
                    }
                }
                else
                {
                    finalStr.Append(fragment);
                }
            }
            if (embedIndex < EmbedExpr.Count)
            {
                throw new InvalidOperationException("Too many embedded expressions provided for FString.");
            }
            Type = typeof(string);
            return finalStr.ToString();
        }

        public string GetText(Runtime runtime)
        {
            return Evaluate(runtime) as string ?? throw new InvalidOperationException("FString evaluation did not return a string.");
        }
    }

    public class UnaryOperationNode : DSExpressionNode
    {
        public UnaryOperator Operator { get; private set; }
        public DSExpressionNode Operand { get; private set; }

        internal UnaryOperationNode(UnaryOperator operatorSymbol, DSExpressionNode operand)
        {
            Operator = operatorSymbol;
            Operand = operand;
        }

        public override string ToString()
        {
            return $"({Operator}{Operand})";
        }

        public override object Evaluate(Runtime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime), "Runtime Environment cannot be null.");
            }

            var operandValue = Operand.Evaluate(runtime);
            switch (Operator)
            {
                case UnaryOperator.Negate:
                    if (Operand.Type == typeof(int) && operandValue is int intValue)
                    {
                        Type = typeof(int);
                        return -intValue;
                    }
                    else if (Operand.Type == typeof(float) && operandValue is float floatValue)
                    {
                        Type = typeof(float);
                        return -floatValue;
                    }
                    else if (Operand.Type == typeof(double) && operandValue is double doubleValue)
                    {
                        Type = typeof(double);
                        return -doubleValue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Negation is not supported for type {Operand.Type}.");
                    }
                case UnaryOperator.Not:
                    if (Operand.Type == typeof(bool) && operandValue is bool boolValue)
                    {
                        Type = typeof(bool);
                        return !boolValue;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Logical NOT is not supported for type {Operand.Type}.");
                    }
                default:
                    throw new NotSupportedException($"Unary operator '{Operator}' is not supported.");
            }
        }
    }

    public class BinaryOperationNode : DSExpressionNode
    {
        public BinaryOperator Operator { get; private set; }
        public DSExpressionNode Left { get; private set; }
        public DSExpressionNode Right { get; private set; }

        internal BinaryOperationNode(BinaryOperator operatorSymbol, DSExpressionNode leftOperand, DSExpressionNode rightOperand)
        {
            Operator = operatorSymbol;
            Left = leftOperand;
            Right = rightOperand;
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }

        public override object Evaluate(Runtime runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime), "Runtime Environment cannot be null.");
            }

            var rightValue = Right.Evaluate(runtime);

            // Handle assignment before evaluating the left operand because it may be a new declaration
            if (Operator == BinaryOperator.Assign)
            {
                if (Left is VariableNode variableNode)
                {
                    runtime.Variables.Set(variableNode.VariableName, rightValue);
                    Type = Right.Type;
                    return Left.Evaluate(runtime);
                }
                else
                {
                    throw new InvalidOperationException("Assignment can only be performed to a variable.");
                }
            }

            var leftValue = Left.Evaluate(runtime);
            switch (Operator)
            {
                case BinaryOperator.Add:
                    if (Left.Type == typeof(string) && leftValue is string leftString && Right.Type == typeof(string) && rightValue is string rightString)
                    {
                        Type = typeof(string);
                        return leftString + rightString;
                    }
                    else
                    {
                        Type = NumTypeChecker(leftValue, rightValue);
                        if (Type == typeof(int))
                        {
                            return Convert.ToInt32(leftValue) + Convert.ToInt32(rightValue);
                        }
                        else if (Type == typeof(float))
                        {
                            return Convert.ToSingle(leftValue) + Convert.ToSingle(rightValue);
                        }
                        else if (Type == typeof(double))
                        {
                            return Convert.ToDouble(leftValue) + Convert.ToDouble(rightValue);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Addition is not supported for types {Left.Type} and {Right.Type}.");
                        }
                    }
                case BinaryOperator.Subtract:
                    Type = NumTypeChecker(leftValue, rightValue);
                    if (Type == typeof(int))
                    {
                        return Convert.ToInt32(leftValue) - Convert.ToInt32(rightValue);
                    }
                    else if (Type == typeof(float))
                    {
                        return Convert.ToSingle(leftValue) - Convert.ToSingle(rightValue);
                    }
                    else if (Type == typeof(double))
                    {
                        return Convert.ToDouble(leftValue) - Convert.ToDouble(rightValue);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Subtraction is not supported for types {Left.Type} and {Right.Type}.");
                    }
                case BinaryOperator.Multiply:
                    Type = NumTypeChecker(leftValue, rightValue);
                    if (Type == typeof(int))
                    {
                        return Convert.ToInt32(leftValue) * Convert.ToInt32(rightValue);
                    }
                    else if (Type == typeof(float))
                    {
                        return Convert.ToSingle(leftValue) * Convert.ToSingle(rightValue);
                    }
                    else if (Type == typeof(double))
                    {
                        return Convert.ToDouble(leftValue) * Convert.ToDouble(rightValue);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Multiplication is not supported for types {Left.Type} and {Right.Type}.");
                    }
                case BinaryOperator.Divide:
                    Type = NumTypeChecker(leftValue, rightValue);
                    if (Type == typeof(int))
                    {
                        if (Convert.ToInt32(rightValue) == 0)
                            throw new DivideByZeroException("Division by zero is not allowed.");
                        return Convert.ToInt32(leftValue) / Convert.ToInt32(rightValue);
                    }
                    else if (Type == typeof(float))
                    {
                        if (Convert.ToSingle(rightValue) == 0f)
                            throw new DivideByZeroException("Division by zero is not allowed.");
                        return Convert.ToSingle(leftValue) / Convert.ToSingle(rightValue);
                    }
                    else if (Type == typeof(double))
                    {
                        if (Convert.ToDouble(rightValue) == 0d)
                            throw new DivideByZeroException("Division by zero is not allowed.");
                        return Convert.ToDouble(leftValue) / Convert.ToDouble(rightValue);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Division is not supported for types {Left.Type} and {Right.Type}.");
                    }
                case BinaryOperator.Modulo:
                    Type = NumTypeChecker(leftValue, rightValue);
                    if (Type == typeof(int))
                    {
                        if (Convert.ToInt32(rightValue) == 0)
                            throw new DivideByZeroException("Division by zero is not allowed.");
                        return Convert.ToInt32(leftValue) % Convert.ToInt32(rightValue);
                    }
                    else if (Type == typeof(float))
                    {
                        if (Convert.ToSingle(rightValue) == 0f)
                            throw new DivideByZeroException("Division by zero is not allowed.");
                        return Convert.ToSingle(leftValue) % Convert.ToSingle(rightValue);
                    }
                    else if (Type == typeof(double))
                    {
                        if (Convert.ToDouble(rightValue) == 0d)
                            throw new DivideByZeroException("Division by zero is not allowed.");
                        return Convert.ToDouble(leftValue) % Convert.ToDouble(rightValue);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Modulus is not supported for types {Left.Type} and {Right.Type}.");
                    }
                case BinaryOperator.Equal:
                    Type = typeof(bool);
                    if (Left.Type == Right.Type)
                    {
                        return Equals(leftValue, rightValue);
                    }
                    else if (Left.Type == typeof(string) && Right.Type == typeof(string))
                    {
                        return string.Equals(leftValue as string, rightValue as string, StringComparison.Ordinal);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Equality check is not supported for types {Left.Type} and {Right.Type}.");
                    }
                case BinaryOperator.NotEqual:
                    Type = typeof(bool);
                    if (Left.Type == Right.Type)
                    {
                        return !Equals(leftValue, rightValue);
                    }
                    else if (Left.Type == typeof(string) && Right.Type == typeof(string))
                    {
                        return !string.Equals(leftValue as string, rightValue as string, StringComparison.Ordinal);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Inequality check is not supported for types {Left.Type} and {Right.Type}.");
                    }
                case BinaryOperator.LessThan:
                    Type = typeof(bool);
                    if (Left.Type == typeof(string) && Right.Type == typeof(string))
                    {
                        return string.Compare(leftValue as string, rightValue as string, StringComparison.Ordinal) < 0;
                    }
                    else
                    {
                        var checkedType = NumTypeChecker(leftValue, rightValue);
                        return checkedType switch
                        {
                            Type t when t == typeof(int) => Convert.ToInt32(leftValue) < Convert.ToInt32(rightValue),
                            Type t when t == typeof(float) => Convert.ToSingle(leftValue) < Convert.ToSingle(rightValue),
                            Type t when t == typeof(double) => Convert.ToDouble(leftValue) < Convert.ToDouble(rightValue),
                            _ => throw new InvalidOperationException($"Less than comparison is not supported for types {Left.Type} and {Right.Type}.")
                        };
                    }
                case BinaryOperator.GreaterThan:
                    Type = typeof(bool);
                    if (Left.Type == typeof(string) && Right.Type == typeof(string))
                    {
                        return string.Compare(leftValue as string, rightValue as string, StringComparison.Ordinal) > 0;
                    }
                    else
                    {
                        var checkedType = NumTypeChecker(leftValue, rightValue);
                        return checkedType switch
                        {
                            Type t when t == typeof(int) => Convert.ToInt32(leftValue) > Convert.ToInt32(rightValue),
                            Type t when t == typeof(float) => Convert.ToSingle(leftValue) > Convert.ToSingle(rightValue),
                            Type t when t == typeof(double) => Convert.ToDouble(leftValue) > Convert.ToDouble(rightValue),
                            _ => throw new InvalidOperationException($"Greater than comparison is not supported for types {Left.Type} and {Right.Type}.")
                        };
                    }
                case BinaryOperator.LessThanOrEqual:
                    Type = typeof(bool);
                    if (Left.Type == typeof(string) && Right.Type == typeof(string))
                    {
                        return string.Compare(leftValue as string, rightValue as string, StringComparison.Ordinal) <= 0;
                    }
                    else
                    {
                        var checkedType = NumTypeChecker(leftValue, rightValue);
                        return checkedType switch
                        {
                            Type t when t == typeof(int) => Convert.ToInt32(leftValue) <= Convert.ToInt32(rightValue),
                            Type t when t == typeof(float) => Convert.ToSingle(leftValue) <= Convert.ToSingle(rightValue),
                            Type t when t == typeof(double) => Convert.ToDouble(leftValue) <= Convert.ToDouble(rightValue),
                            _ => throw new InvalidOperationException($"Less than or equal comparison is not supported for types {Left.Type} and {Right.Type}.")
                        };
                    }
                case BinaryOperator.GreaterThanOrEqual:
                    Type = typeof(bool);
                    if (Left.Type == typeof(string) && Right.Type == typeof(string))
                    {
                        return string.Compare(leftValue as string, rightValue as string, StringComparison.Ordinal) >= 0;
                    }
                    else
                    {
                        var checkedType = NumTypeChecker(leftValue, rightValue);
                        return checkedType switch
                        {
                            Type t when t == typeof(int) => Convert.ToInt32(leftValue) >= Convert.ToInt32(rightValue),
                            Type t when t == typeof(float) => Convert.ToSingle(leftValue) >= Convert.ToSingle(rightValue),
                            Type t when t == typeof(double) => Convert.ToDouble(leftValue) >= Convert.ToDouble(rightValue),
                            _ => throw new InvalidOperationException($"Greater than or equal comparison is not supported for types {Left.Type} and {Right.Type}.")
                        };
                    }
                case BinaryOperator.AndAlso:
                    if (Left.Type != typeof(bool) || Right.Type != typeof(bool))
                    {
                        throw new InvalidOperationException($"Logical AND is only supported for boolean types, but got {Left.Type} and {Right.Type}.");
                    }
                    Type = typeof(bool);
                    return Convert.ToBoolean(leftValue) && Convert.ToBoolean(rightValue);
                case BinaryOperator.OrElse:
                    if (Left.Type != typeof(bool) || Right.Type != typeof(bool))
                    {
                        throw new InvalidOperationException($"Logical OR is only supported for boolean types, but got {Left.Type} and {Right.Type}.");
                    }
                    Type = typeof(bool);
                    return Convert.ToBoolean(leftValue) || Convert.ToBoolean(rightValue);
                default:
                    throw new NotSupportedException($"Binary operator '{Operator}' is not supported.");
            }
        }

        private static Type NumTypeChecker(object leftValue, object rightValue)
        {
            bool isLeftInt = leftValue is int;
            bool isLeftFloat = leftValue is float;
            bool isLeftDouble = leftValue is double;

            bool isRightInt = rightValue is int;
            bool isRightFloat = rightValue is float;
            bool isRightDouble = rightValue is double;

            if ((!isLeftInt && !isLeftFloat && !isLeftDouble) ||
                (!isRightInt && !isRightFloat && !isRightDouble))
            {
                throw new ArgumentException("Both values must be of type int, float, or double.");
            }

            if (isLeftDouble || isRightDouble)
            {
                return typeof(double);
            }
            else if (isLeftFloat || isRightFloat)
            {
                return typeof(float);
            }
            else
            {
                return typeof(int);
            }
        }
    }

    public enum UnaryOperator
    {
        Negate,
        Not,
    }

    public enum BinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        AndAlso,
        OrElse,
        Assign
    }
}