using UnityEngine;
using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using UnityEditor.Rendering.Canvas.ShaderGraph;
using Mono.Cecil.Cil;
using NUnit.Framework.Internal;

namespace Assets.Scripts.DSP.Core
{
    public class Interpreter : MonoBehaviour
    {
        // public Action<IR_Dialogue> OnDialogue;

        private readonly Dictionary<string, (object Value, Type Type)> _variableDict = new();

        private readonly Dictionary<string, Delegate> _functionDict = new();

        public void SetVariable(string name, object value, Type type)
            => _variableDict[name] = (value, type);

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

        # region Function Registration
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