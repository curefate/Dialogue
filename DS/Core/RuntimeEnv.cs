namespace DS.Core
{
    public class RuntimeEnv
    {
        public readonly VariableRegistry Variables = new();
        public readonly FunctionRegistry Functions = new();

        private readonly Dictionary<string, LabelBlock> labelHub = [];
        private readonly LinkedList<IRInstruction> executionQueue = new();

        public bool HasNext => executionQueue.Count > 0;

        public void Read(LabelBlock block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block), "LabelBlock cannot be null");
            }
            if (labelHub.ContainsKey(block.LabelName))
            {
                throw new InvalidOperationException($"Label '{block.LabelName}' already exists in the LabelHub.");
            }
            labelHub[block.LabelName] = block;
        }

        public void Read(List<LabelBlock> blocks)
        {
            if (blocks == null || blocks.Count == 0)
            {
                throw new ArgumentException("The list of LabelBlocks cannot be null or empty.", nameof(blocks));
            }
            foreach (var block in blocks)
            {
                Read(block);
            }
        }

        public LabelBlock GetLabelBlock(string labelName)
        {
            if (string.IsNullOrEmpty(labelName))
            {
                throw new ArgumentException("Label name cannot be null or empty.", nameof(labelName));
            }
            if (!labelHub.TryGetValue(labelName, out var block))
            {
                throw new KeyNotFoundException($"Label '{labelName}' not found.");
            }
            return block;
        }

        public void Load(string LabelName = "start")
        {
            if (string.IsNullOrEmpty(LabelName))
            {
                throw new ArgumentException("Label name cannot be null or empty.", nameof(LabelName));
            }
            if (!labelHub.ContainsKey(LabelName))
            {
                throw new KeyNotFoundException($"Label '{LabelName}' not found in the LabelHub.");
            }
            executionQueue.Clear();
            Enqueue(labelHub[LabelName].Instructions);
        }

        public void ClearLabels()
        {
            labelHub.Clear();
        }

        public void Enqueue(IRInstruction instruction, bool fromHead = false)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException(nameof(instruction), "IRInstruction cannot be null");
            }
            if (fromHead)
            {
                executionQueue.AddFirst(instruction);
            }
            else
            {
                executionQueue.AddLast(instruction);
            }
        }

        public void Enqueue(List<IRInstruction> instructions, bool fromHead = false)
        {
            if (instructions == null || instructions.Count == 0)
            {
                throw new ArgumentException("The list of IRInstructions cannot be null or empty.", nameof(instructions));
            }
            if (fromHead)
            {
                for (int i = instructions.Count - 1; i >= 0; i--)
                {
                    Enqueue(instructions[i], true);
                }
            }
            else
            {
                foreach (var instruction in instructions)
                {
                    Enqueue(instruction, false);
                }
            }
        }

        public IRInstruction? LA(int offset = 0)
        {
            if (offset < 0 || offset >= executionQueue.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is out of range of the execution queue.");
            }
            if (executionQueue.Count == 0)
            {
                return null;
            }
            var node = executionQueue.First ?? throw new InvalidOperationException("ExecutionQueue is empty.");
            for (int i = 0; i < offset; i++)
            {
                node = node.Next ?? throw new InvalidOperationException("Offset exceeds the bounds of the execution queue.");
            }
            return node.Value;
        }

        public IRInstruction Pop()
        {
            if (executionQueue.Count == 0)
            {
                throw new InvalidOperationException("ExecutionQueue is empty. Cannot pop an instruction.");
            }
            if (executionQueue.First == null)
            {
                throw new InvalidOperationException("ExecutionQueue is empty. Cannot pop an instruction.");
            }
            var instruction = executionQueue.First.Value;
            executionQueue.RemoveFirst();
            return instruction;
        }

        public void ClearQueue()
        {
            executionQueue.Clear();
        }
    }

    public class VariableRegistry
    {
        internal VariableRegistry() { }

        private readonly Dictionary<string, TypedVar> variables = [];

        public void Clear() => variables.Clear();

        public void Set(string varName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            var type = value.GetType();
            if (type == typeof(TypedVar))
            {
                var typedValue = (TypedVar)value;
                type = typedValue.Type;
                value = typedValue.Value;
            }
            if (type != typeof(string) && type != typeof(int) && type != typeof(float) && type != typeof(bool))
            {
                throw new ArgumentException($"Unsupported type '{type.Name}' for variable '{varName}'. Supported types are string, int, float, and bool.", nameof(value));
            }
            if (variables.ContainsKey(varName))
            {
                // Check if the type matches the existing variable
                if (variables[varName].Type != type)
                {
                    throw new InvalidOperationException($"Variable '{varName}' already exists with type '{variables[varName].Type.Name}', cannot assign value of type '{type.Name}'. Supported types are string, int, float, and bool.");
                }
                variables[varName] = new TypedVar(value, type);
            }
            else
            {
                variables.Add(varName, new TypedVar(value, type));
            }
        }

        public TypedVar Get(string varName)
        {
            if (variables.TryGetValue(varName, out var variable))
            {
                return variable;
            }
            throw new KeyNotFoundException($"Variable '{varName}' not found in the interpreter's variable dictionary.");
        }
    }

    public class FunctionRegistry
    {
        internal FunctionRegistry() { }
        private readonly Dictionary<string, Delegate> functions = [];
        public void Clear() => functions.Clear();
        public void AddFunction<TResult>(string funcName, Func<TResult> func) => functions[funcName] = func;
        public void AddFunction<T0, TResult>(string funcName, Func<T0, TResult> func) => functions[funcName] = func;
        public void AddFunction<T0, T1, TResult>(string funcName, Func<T0, T1, TResult> func) => functions[funcName] = func;
        public void AddFunction<T0, T1, T2, TResult>(string funcName, Func<T0, T1, T2, TResult> func) => functions[funcName] = func;
        public void AddFunction<T0, T1, T2, T3, TResult>(string funcName, Func<T0, T1, T2, T3, TResult> func) => functions[funcName] = func;
        public void AddFunction<T0, T1, T2, T3, T4, TResult>(string funcName, Func<T0, T1, T2, T3, T4, TResult> func) => functions[funcName] = func;
        public void AddFunction(string funcName, Action action) => functions[funcName] = action;
        public void AddFunction<T0>(string funcName, Action<T0> action) => functions[funcName] = action;
        public void AddFunction<T0, T1>(string funcName, Action<T0, T1> action) => functions[funcName] = action;
        public void AddFunction<T0, T1, T2>(string funcName, Action<T0, T1, T2> action) => functions[funcName] = action;
        public void AddFunction<T0, T1, T2, T3>(string funcName, Action<T0, T1, T2, T3> action) => functions[funcName] = action;
        public void AddFunction<T0, T1, T2, T3, T4>(string funcName, Action<T0, T1, T2, T3, T4> action) => functions[funcName] = action;
        public Delegate GetDelegate(string funcName)
        {
            if (functions.TryGetValue(funcName, out var func))
            {
                return func;
            }
            throw new KeyNotFoundException($"Function '{funcName}' not found.");
        }
        public dynamic? Invoke(string funcName, params object[] args)
        {
            if (functions.TryGetValue(funcName, out var func))
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
            if (functions.TryGetValue(funcName, out var func) && func is Func<TResult> function)
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
            if (functions.TryGetValue(funcName, out var func) && func is Func<T0, TResult> function)
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
            if (functions.TryGetValue(funcName, out var func) && func is Func<T0, T1, TResult> function)
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
            if (functions.TryGetValue(funcName, out var func) && func is Func<T0, T1, T2, TResult> function)
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
            if (functions.TryGetValue(funcName, out var func) && func is Func<T0, T1, T2, T3, TResult> function)
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
            if (functions.TryGetValue(funcName, out var func) && func is Func<T0, T1, T2, T3, T4, TResult> function)
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
            if (functions.TryGetValue(funcName, out var func) && func is Action action)
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
            if (functions.TryGetValue(funcName, out var func) && func is Action<T0> action)
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
            if (functions.TryGetValue(funcName, out var func) && func is Action<T0, T1> action)
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
            if (functions.TryGetValue(funcName, out var func) && func is Action<T0, T1, T2> action)
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
            if (functions.TryGetValue(funcName, out var func) && func is Action<T0, T1, T2, T3> action)
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
            if (functions.TryGetValue(funcName, out var func) && func is Action<T0, T1, T2, T3, T4> action)
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
    }

    public class TypedVar
    {
        public object Value { get; }
        public Type Type { get; }

        internal TypedVar(object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Type = value.GetType();
        }

        internal TypedVar(object value, Type type)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            Type = type;
        }
    }
}