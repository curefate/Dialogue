using UnityEngine;
using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using UnityEditor.Rendering.Canvas.ShaderGraph;
using Mono.Cecil.Cil;
using NUnit.Framework.Internal;
using System.Linq.Expressions;

namespace Assets.Scripts.DSP.Core
{
    public class Interpreter : MonoBehaviour
    {
        // Events
        public Action<IR_Dialogue> OnDialogue;
        public Action<IR_Menu> OnMenu;

        #region Expression Evaluation
        public TypedValue EvaluateExpression(Expression expression)
        {
            // TODO stricter type checking and error handling
            switch (expression)
            {
                case ConstantExpression constantExpr:
                    // 处理变量名和常量
                    var typedVar = constantExpr.Value as TypedValue;
                    if (typedVar.Type == typeof(object) && typedVar.Value is string strValue && strValue.StartsWith("$"))
                    {
                        return GetVariable(strValue[1..]);
                    }
                    return typedVar;

                case BinaryExpression binaryExpr:
                    var left = EvaluateExpression(binaryExpr.Left);
                    var right = EvaluateExpression(binaryExpr.Right);

                    // 处理不同类型运算
                    switch (binaryExpr.NodeType)
                    {
                        case ExpressionType.Equal:
                            return new TypedValue(Equals(left.Value, right.Value), typeof(bool));
                        case ExpressionType.NotEqual:
                            return new TypedValue(!Equals(left.Value, right.Value), typeof(bool));
                        case ExpressionType.AndAlso:
                            return new TypedValue(Convert.ToBoolean(left.Value) && Convert.ToBoolean(right.Value), typeof(bool));
                        case ExpressionType.OrElse:
                            return new TypedValue(Convert.ToBoolean(left.Value) || Convert.ToBoolean(right.Value), typeof(bool));
                        case ExpressionType.GreaterThan:
                            if (left.Type == typeof(string) && right.Type == typeof(string))
                            {
                                return new TypedValue(string.Compare((string)left.Value, (string)right.Value) > 0, typeof(bool));
                            }
                            else if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) > Convert.ToSingle(right.Value), typeof(bool));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) > Convert.ToInt32(right.Value), typeof(bool));
                            }
                        case ExpressionType.GreaterThanOrEqual:
                            if (left.Type == typeof(string) && right.Type == typeof(string))
                            {
                                return new TypedValue(string.Compare((string)left.Value, (string)right.Value) >= 0, typeof(bool));
                            }
                            else if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) >= Convert.ToSingle(right.Value), typeof(bool));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) >= Convert.ToInt32(right.Value), typeof(bool));
                            }
                        case ExpressionType.LessThan:
                            if (left.Type == typeof(string) && right.Type == typeof(string))
                            {
                                return new TypedValue(string.Compare((string)left.Value, (string)right.Value) < 0, typeof(bool));
                            }
                            else if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) < Convert.ToSingle(right.Value), typeof(bool));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) < Convert.ToInt32(right.Value), typeof(bool));
                            }
                        case ExpressionType.LessThanOrEqual:
                            if (left.Type == typeof(string) && right.Type == typeof(string))
                            {
                                return new TypedValue(string.Compare((string)left.Value, (string)right.Value) <= 0, typeof(bool));
                            }
                            else if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) <= Convert.ToSingle(right.Value), typeof(bool));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) <= Convert.ToInt32(right.Value), typeof(bool));
                            }
                        case ExpressionType.Add:
                            if (left.Type == typeof(string) && right.Type == typeof(string))
                            {
                                return new TypedValue((string)left.Value + (string)right.Value, typeof(string));
                            }
                            else if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) + Convert.ToSingle(right.Value), typeof(float));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) + Convert.ToInt32(right.Value), typeof(int));
                            }
                        case ExpressionType.Subtract:
                            if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) - Convert.ToSingle(right.Value), typeof(float));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) - Convert.ToInt32(right.Value), typeof(int));
                            }
                        case ExpressionType.Multiply:
                            if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) * Convert.ToSingle(right.Value), typeof(float));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) * Convert.ToInt32(right.Value), typeof(int));
                            }
                        case ExpressionType.Divide:
                            if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) / Convert.ToSingle(right.Value), typeof(float));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) / Convert.ToInt32(right.Value), typeof(int));
                            }
                        case ExpressionType.Modulo:
                            if (left.Type == typeof(float) || right.Type == typeof(float))
                            {
                                return new TypedValue(Convert.ToSingle(left.Value) % Convert.ToSingle(right.Value), typeof(float));
                            }
                            else
                            {
                                return new TypedValue(Convert.ToInt32(left.Value) % Convert.ToInt32(right.Value), typeof(int));
                            }
                        // TODO more operators such pow, etc.
                        default:
                            throw new NotSupportedException($"Unsupported binary operation: {binaryExpr.NodeType}");
                    }
                    ;

                case UnaryExpression unaryExpr:
                    var operand = EvaluateExpression(unaryExpr.Operand);
                    switch (unaryExpr.NodeType)
                    {
                        case ExpressionType.Negate:
                            if (operand.Type == typeof(float))
                            {
                                return new TypedValue(-Convert.ToSingle(operand.Value), typeof(float));
                            }
                            else
                            {
                                return new TypedValue(-Convert.ToInt32(operand.Value), typeof(int));
                            }
                        case ExpressionType.Not:
                            return new TypedValue(!Convert.ToBoolean(operand.Value), typeof(bool));
                        default:
                            throw new NotSupportedException($"Unsupported unary operation: {unaryExpr.NodeType}");
                    }
                    ;

                default:
                    throw new NotSupportedException($"Unsupport expresstion type: {expression.GetType().Name}");
            }
        }
        #endregion

        #region Runtime
        public readonly List<InstructionBlock> _labelBlocks = new(); //TODO dic?
        public readonly Stack<IIRInstruction> _runStack = new();

        public void Run(string labelName = "start")
        {
            var label = _labelBlocks.Find(l => l.LabelName == labelName);
            if (label != null)
            {
                _runStack.Clear();
                foreach (var instruction in label.Instructions)
                {
                    _runStack.Push(instruction);
                }
                Next();
            }
            else
            {
                Debug.LogError($"Label '{labelName}' not found.");
                throw new KeyNotFoundException($"Label '{labelName}' not found in the interpreter's label blocks.");
            }
        }

        public void Next()
        {
            if (_runStack.Count > 0)
            {
                var instruction = _runStack.Pop();
                instruction.Execute(this);
            }
            else
            {
                Debug.LogWarning("No instructions to execute.");
                throw new InvalidOperationException("No instructions left in the stack to execute.");
            }
        }
        #endregion

        #region Variable Registration
        private readonly Dictionary<string, TypedValue> _variableDict = new();
        public bool ContainsVariable(string name) => _variableDict.ContainsKey(name);
        public void SetVariable(string name, object value)
        {
            if (value == null)
            {
                Debug.LogError($"Cannot set variable '{name}' to null.");
                return;
            }
            var type = value.GetType();
            if (type == typeof(TypedValue))
            {
                var typedValue = (TypedValue)value;
                type = typedValue.Type;
                value = typedValue.Value;
            }
            if (type != typeof(string) && type != typeof(int) && type != typeof(float) && type != typeof(bool))
            {
                Debug.LogError($"Unsupported variable type '{type.Name}' for variable '{name}'. Only string, int, float, and bool are allowed.");
                return;
            }
            if (_variableDict.ContainsKey(name))
            {
                // Check if the type matches the existing variable
                if (_variableDict[name].Type != type)
                {
                    Debug.LogError($"Variable '{name}' already exists with a different type '{_variableDict[name].Type.Name}'. Cannot change type.");
                    return;
                }
                _variableDict[name] = new TypedValue(value, type);
            }
            else
            {
                _variableDict.Add(name, new TypedValue(value, type));
            }
        }
        public TypedValue GetVariable(string name)
        {
            if (_variableDict.TryGetValue(name, out var variable))
            {
                return variable;
            }
            Debug.LogError($"Variable '{name}' not found.");
            return null;
        }
        #endregion

        #region Function Registration
        private readonly Dictionary<string, Delegate> _functionDict = new();
        public void AddFunction<TResult>(string funcName, Func<TResult> func) => _functionDict[funcName] = func;
        public void AddFunction<T0, TResult>(string funcName, Func<T0, TResult> func) => _functionDict[funcName] = func;
        public void AddFunction<T0, T1, TResult>(string funcName, Func<T0, T1, TResult> func) => _functionDict[funcName] = func;
        public void AddFunction<T0, T1, T2, TResult>(string funcName, Func<T0, T1, T2, TResult> func) => _functionDict[funcName] = func;
        public void AddFunction<T0, T1, T2, T3, TResult>(string funcName, Func<T0, T1, T2, T3, TResult> func) => _functionDict[funcName] = func;
        public void AddFunction<T0, T1, T2, T3, T4, TResult>(string funcName, Func<T0, T1, T2, T3, T4, TResult> func) => _functionDict[funcName] = func;
        public void AddFunction(string funcName, Action action) => _functionDict[funcName] = action;
        public void AddFunction<T0>(string funcName, Action<T0> action) => _functionDict[funcName] = action;
        public void AddFunction<T0, T1>(string funcName, Action<T0, T1> action) => _functionDict[funcName] = action;
        public void AddFunction<T0, T1, T2>(string funcName, Action<T0, T1, T2> action) => _functionDict[funcName] = action;
        public void AddFunction<T0, T1, T2, T3>(string funcName, Action<T0, T1, T2, T3> action) => _functionDict[funcName] = action;
        public void AddFunction<T0, T1, T2, T3, T4>(string funcName, Action<T0, T1, T2, T3, T4> action) => _functionDict[funcName] = action;
        public dynamic Invoke(string funcName, params object[] args)
        {
            if (_functionDict.TryGetValue(funcName, out var func))
            {
                try
                {
                    return func.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return null;
                }
            }
            Debug.LogError($"Function '{funcName}' not found.");
            return null;
        }
        public TResult Invoke<TResult>(string funcName)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Func<TResult> function)
            {
                try
                {
                    return function();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return default;
                }
            }
            Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            return default;
        }
        public TResult Invoke<T0, TResult>(string funcName, T0 arg0)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Func<T0, TResult> function)
            {
                try
                {
                    return function(arg0);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return default;
                }
            }
            Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            return default;
        }
        public TResult Invoke<T0, T1, TResult>(string funcName, T0 arg0, T1 arg1)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Func<T0, T1, TResult> function)
            {
                try
                {
                    return function(arg0, arg1);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return default;
                }
            }
            Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            return default;
        }
        public TResult Invoke<T0, T1, T2, TResult>(string funcName, T0 arg0, T1 arg1, T2 arg2)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Func<T0, T1, T2, TResult> function)
            {
                try
                {
                    return function(arg0, arg1, arg2);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return default;
                }
            }
            Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            return default;
        }
        public TResult Invoke<T0, T1, T2, T3, TResult>(string funcName, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Func<T0, T1, T2, T3, TResult> function)
            {
                try
                {
                    return function(arg0, arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return default;
                }
            }
            Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            return default;
        }
        public TResult Invoke<T0, T1, T2, T3, T4, TResult>(string funcName, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Func<T0, T1, T2, T3, T4, TResult> function)
            {
                try
                {
                    return function(arg0, arg1, arg2, arg3, arg4);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                    return default;
                }
            }
            Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            return default;
        }
        public void Invoke(string funcName)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Action action)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        public void Invoke<T0>(string funcName, T0 arg0)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Action<T0> action)
            {
                try
                {
                    action(arg0);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        public void Invoke<T0, T1>(string funcName, T0 arg0, T1 arg1)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Action<T0, T1> action)
            {
                try
                {
                    action(arg0, arg1);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        public void Invoke<T0, T1, T2>(string funcName, T0 arg0, T1 arg1, T2 arg2)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Action<T0, T1, T2> action)
            {
                try
                {
                    action(arg0, arg1, arg2);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        public void Invoke<T0, T1, T2, T3>(string funcName, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Action<T0, T1, T2, T3> action)
            {
                try
                {
                    action(arg0, arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        public void Invoke<T0, T1, T2, T3, T4>(string funcName, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (_functionDict.TryGetValue(funcName, out var func) && func is Action<T0, T1, T2, T3, T4> action)
            {
                try
                {
                    action(arg0, arg1, arg2, arg3, arg4);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error invoking function '{funcName}': {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        # endregion
    }

    public class TypedValue
    {
        public object Value { get; }
        public Type Type { get; }

        public TypedValue(object value, Type type)
        {
            if (type != typeof(string) && type != typeof(int) && type != typeof(float) && type != typeof(bool) && type != typeof(object))
            {
                throw new ArgumentException($"Unsupported variable type '{type.Name}'. Only string, int, float, and bool are allowed.", nameof(type));
            }
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Type = type;
        }
    }
}