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

        // Evaluate expressions
        #region Expression Evaluation
        public object EvaluateExpression(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression constantExpr:
                    // 处理变量名和常量
                    if (constantExpr.Value is string strValue && expression.Type == typeof(string) && strValue.StartsWith("$"))
                    {
                        return GetVariableValue(strValue[1..]);
                    }
                    return constantExpr.Value;

                case BinaryExpression binaryExpr:
                    var left = EvaluateExpression(binaryExpr.Left);
                    var right = EvaluateExpression(binaryExpr.Right);

                    // 处理不同类型运算
                    return binaryExpr.NodeType switch
                    {
                        ExpressionType.Equal => Equals(left, right),
                        ExpressionType.NotEqual => !Equals(left, right),
                        ExpressionType.GreaterThan => Convert.ToDouble(left) > Convert.ToDouble(right),
                        ExpressionType.LessThan => Convert.ToDouble(left) < Convert.ToDouble(right),
                        ExpressionType.AndAlso => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                        ExpressionType.OrElse => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                        ExpressionType.Add => HandleAdd(left, right),
                        ExpressionType.Subtract => Convert.ToDouble(left) - Convert.ToDouble(right),
                        ExpressionType.Multiply => Convert.ToDouble(left) * Convert.ToDouble(right),
                        ExpressionType.Divide => Convert.ToDouble(left) / Convert.ToDouble(right),
                        ExpressionType.Modulo => Convert.ToDouble(left) % Convert.ToDouble(right),
                        // TODO more operators such pow, +=, etc.
                        _ => throw new NotSupportedException($"不支持的二元运算符: {binaryExpr.NodeType}")
                    };

                case UnaryExpression unaryExpr:
                    var operand = EvaluateExpression(unaryExpr.Operand);
                    return unaryExpr.NodeType switch
                    {
                        ExpressionType.Negate => -Convert.ToDouble(operand),
                        ExpressionType.Not => !Convert.ToBoolean(operand),
                        _ => throw new NotSupportedException($"不支持的一元运算符: {unaryExpr.NodeType}")
                    };

                default:
                    throw new NotSupportedException($"不支持的表达式类型: {expression.GetType().Name}");
            }
        }

        private object HandleAdd(object left, object right)
        {
            // 处理字符串拼接或数字加法
            if (left is string || right is string)
            {
                return $"{left}{right}";
            }
            return Convert.ToDouble(left) + Convert.ToDouble(right);
        }
        #endregion

        // Temporary storage for running
        #region runtime
        public readonly List<LabelBlock> _labelBlocks = new(); //TODO dic?
        public readonly Stack<IIRInstruction> _instructionStack = new();

        public void Run(string labelName = "start")
        {
            var label = _labelBlocks.Find(l => l.LabelName == labelName);
            if (label != null)
            {
                _instructionStack.Clear();
                foreach (var instruction in label.Instructions)
                {
                    _instructionStack.Push(instruction);
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
            if (_instructionStack.Count > 0)
            {
                var instruction = _instructionStack.Pop();
                instruction.Execute(this);
            }
            else
            {
                Debug.LogWarning("No instructions to execute.");
                throw new InvalidOperationException("No instructions left in the stack to execute.");
            }
        }
        #endregion

        // Variable Registration
        #region Variable Registration
        private readonly Dictionary<string, (object Value, Type Type)> _variableDict = new();
        public bool ContainsVariable(string name) => _variableDict.ContainsKey(name);
        public void SetVariable(string name, object value)
        {
            if (value == null)
            {
                Debug.LogError($"Cannot set variable '{name}' to null.");
                return;
            }
            var type = value.GetType();
            if (type != typeof(string) && type != typeof(int) && type != typeof(float) && type != typeof(bool))
            {
                Debug.LogError($"Unsupported variable type '{type.Name}' for variable '{name}'. Only string, int, float, and bool are allowed.");
                return;
            }
            if (_variableDict.ContainsKey(name))
            {
                _variableDict[name] = (value, type);
            }
            else
            {
                _variableDict.Add(name, (value, type));
            }
        }
        public (object Value, Type Type) GetVariable(string name)
        {
            if (_variableDict.TryGetValue(name, out var value))
            {
                return value;
            }
            Debug.LogError($"Variable '{name}' not found.");
            return (null, null);
        }
        public object GetVariableValue(string name)
        {
            if (_variableDict.TryGetValue(name, out var value))
            {
                return value.Value;
            }
            Debug.LogError($"Variable '{name}' not found.");
            return null;
        }
        public Type GetVariableType(string name)
        {
            if (_variableDict.TryGetValue(name, out var value))
            {
                return value.Type;
            }
            Debug.LogError($"Variable '{name}' not found.");
            return null;
        }
        #endregion

        // Function Registration
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
}