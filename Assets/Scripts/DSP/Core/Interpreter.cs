using UnityEngine;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.DSP.Core
{
    public class Interpreter : MonoBehaviour
    {
        // Events
        public Action<IR_Dialogue> OnDialogue;
        public Func<IR_Menu, int> OnMenu;

        #region Runtime
        public readonly List<InstructionBlock> LabelBlocks = new(); //TODO dic?
        public readonly LinkedList<IRInstruction> RunningQueue = new();

        public void Run(string labelName = "start")
        {
            var block = LabelBlocks.Find(l => l.LabelName == labelName);
            if (block != null)
            {
                RunningQueue.Clear();
                foreach (var instruction in block.Instructions)
                {
                    RunningQueue.AddLast(instruction);
                }
                Next();
            }
            else
            {
                throw new KeyNotFoundException($"Label '{labelName}' not found in the interpreter's label blocks.");
            }
        }

        public void Next()
        {
            if (RunningQueue.Count > 0)
            {
                var instruction = RunningQueue.First.Value;
                RunningQueue.RemoveFirst();
                instruction.Execute(this);
            }
            else
            {
                throw new InvalidOperationException("No instructions left in the stack to execute.");
            }
        }
        #endregion

        #region Variable Registration
        private readonly Dictionary<string, TypedVariable> _variableDict = new();
        public bool ContainsVariable(string name) => _variableDict.ContainsKey(name);
        public void SetVariable(string name, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            var type = value.GetType();
            if (type == typeof(TypedVariable))
            {
                var typedValue = (TypedVariable)value;
                type = typedValue.Type;
                value = typedValue.Value;
            }
            if (type != typeof(string) && type != typeof(int) && type != typeof(float) && type != typeof(bool))
            {
                throw new ArgumentException($"Unsupported type '{type.Name}' for variable '{name}'. Supported types are string, int, float, and bool.", nameof(value));
            }
            if (_variableDict.ContainsKey(name))
            {
                // Check if the type matches the existing variable
                if (_variableDict[name].Type != type)
                {
                    throw new InvalidOperationException($"Variable '{name}' already exists with type '{_variableDict[name].Type.Name}', cannot assign value of type '{type.Name}'. Supported types are string, int, float, and bool.");
                }
                _variableDict[name] = new TypedVariable(value, type);
            }
            else
            {
                _variableDict.Add(name, new TypedVariable(value, type));
            }
        }
        public TypedVariable GetVariable(string name)
        {
            if (_variableDict.TryGetValue(name, out var variable))
            {
                return variable;
            }
            throw new KeyNotFoundException($"Variable '{name}' not found in the interpreter's variable dictionary.");
        }
        public object GetVariableValue(string name)
        {
            if (_variableDict.TryGetValue(name, out var variable))
            {
                return variable.Value;
            }
            throw new KeyNotFoundException($"Variable '{name}' not found in the interpreter's variable dictionary.");
        }
        public T GetVariableValue<T>(string name)
        {
            if (_variableDict.TryGetValue(name, out var variable))
            {
                if (variable.Type is T && variable.Value is T value)
                {
                    return value;
                }
                throw new InvalidCastException($"Variable '{name}' is of type '{variable.Type.Name}', cannot cast to '{typeof(T).Name}'. Supported types are string, int, float, and bool.");
            }
            throw new KeyNotFoundException($"Variable '{name}' not found in the interpreter's variable dictionary.");
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
        public Delegate GetDelegate(string funcName)
        {
            if (_functionDict.TryGetValue(funcName, out var func))
            {
                return func;
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found.");
        }
        public dynamic Invoke(string funcName, params object[] args)
        {
            if (_functionDict.TryGetValue(funcName, out var func))
            {
                try
                {
                    if (args == null || args.Length == 0)
                    {
                        if (func is Action action)
                        {
                            action();
                            return null;
                        }
                        else if (func is Delegate del && del.Method.GetParameters().Length == 0)
                        {
                            return del.DynamicInvoke();
                        }
                    }
                    return func.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
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
                    throw new InvalidOperationException($"Error invoking function '{funcName}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Function '{funcName}' not found or has incorrect signature.");
            }
        }
        # endregion
    }

    public class TypedVariable
    {
        public object Value { get; }
        public Type Type { get; }

        public TypedVariable(object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Type = value.GetType();
        }

        public TypedVariable(object value, Type type)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Type = type;
        }
    }
}