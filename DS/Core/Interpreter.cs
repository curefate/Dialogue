namespace DS.Core
{
    using System;

    public abstract class IRExecuter
    {
        public virtual void Execute(IRInstruction instruction, RuntimeEnv runtime)
        {
            switch (instruction)
            {
                case IR_Dialogue dialogue:
                    ExecuteDialogue(dialogue, runtime);
                    break;
                case IR_Menu menu:
                    ExecuteMenu(menu, runtime);
                    break;
                case IR_Jump jump:
                    ExecuteJump(jump, runtime);
                    break;
                case IR_Tour tour:
                    ExecuteTour(tour, runtime);
                    break;
                case IR_Call call:
                    ExecuteCall(call, runtime);
                    break;
                case IR_Set set:
                    ExecuteSet(set, runtime);
                    break;
                case IR_If ifInstruction:
                    ExecuteIf(ifInstruction, runtime);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported instruction type: {instruction.GetType().Name}");
            }
        }

        public abstract void ExecuteDialogue(IR_Dialogue instruction, RuntimeEnv runtime);

        public abstract void ExecuteMenu(IR_Menu instruction, RuntimeEnv runtime);

        public virtual void ExecuteJump(IR_Jump instruction, RuntimeEnv runtime)
        {
            try
            {
                var block = runtime.GetLabelBlock(instruction.TargetLabel);
                runtime.ClearQueue();
                runtime.Enqueue(block.Instructions);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Label '{instruction.TargetLabel}' not found.[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public virtual void ExecuteTour(IR_Tour instruction, RuntimeEnv runtime)
        {
            try
            {
                var block = runtime.GetLabelBlock(instruction.TargetLabel);
                runtime.Enqueue(block.Instructions, true);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Label '{instruction.TargetLabel}' not found.[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public virtual void ExecuteCall(IR_Call instruction, RuntimeEnv runtime)
        {
            try
            {
                var args = instruction.Arguments.Select(arg => arg.Evaluate(runtime)).ToArray();
                runtime.Functions.Invoke(instruction.FunctionName, args);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"Function '{instruction.FunctionName}' not found.[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to call function '{instruction.FunctionName}'. {ex.Message} [Ln {instruction.LineNum}, Fp {instruction.FilePath}]", ex);
            }
        }
        public virtual void ExecuteSet(IR_Set instruction, RuntimeEnv runtime)
        {
            try
            {
                var evaluatedValue = instruction.Value.Evaluate(runtime);
                var symbol = instruction.Symbol;
                switch (symbol)
                {
                    case "=":
                        runtime.Variables.Set(instruction.VariableName[1..], evaluatedValue);
                        return;
                    // TODO ADD +=, -=, etc.
                    default:
                        throw new NotSupportedException($"Symbol '{symbol}' is not supported.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set variable '{instruction.VariableName}' with symbol '{instruction.Symbol}'. {ex.Message} [Ln {instruction.LineNum}, Fp {instruction.FilePath}]", ex);
            }

        }
        public virtual void ExecuteIf(IR_If instruction, RuntimeEnv runtime)
        {
            try
            {
                var conditionResult = instruction.Condition.Evaluate(runtime);
                if (conditionResult == null || conditionResult is not bool)
                {
                    throw new InvalidOperationException($"Condition must evaluate to a boolean value.");
                }
                if ((bool)conditionResult)
                {
                    runtime.Enqueue(instruction.TrueBranch, true);
                }
                else
                {
                    runtime.Enqueue(instruction.FalseBranch, true);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to evaluate if condition. {ex.Message} [Ln {instruction.LineNum}, Fp {instruction.FilePath}]", ex);
            }
        }
    }

    public class Interpreter : IRExecuter
    {
        public RuntimeEnv Runtime { get; private set; } = new();
        protected readonly Compiler compiler = new();

        public override void ExecuteDialogue(IR_Dialogue instruction, RuntimeEnv runtime)
        {
            try
            {
                Console.WriteLine($"{(instruction.HasSpeaker ? instruction.SpeakerName + ": " : "")}{instruction.TextNode.Evaluate(runtime)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing dialogue: {ex.Message}[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public override void ExecuteMenu(IR_Menu instruction, RuntimeEnv runtime)
        {
            try
            {
                Console.WriteLine("=====================");
                Console.WriteLine("Menu:");
                int index = 0;
                foreach (var textNode in instruction.OptionTextNodes)
                {
                    Console.WriteLine($"{index++}: " + textNode.Evaluate(runtime));
                }
                Console.Write("Select an option (0-" + (instruction.OptionTextNodes.Count - 1) + "): ");
                var input = Console.ReadLine();
                int choice;
                while (string.IsNullOrEmpty(input) || !int.TryParse(input, out choice) || choice < 0 || choice >= instruction.OptionTextNodes.Count)
                {
                    Console.Write("Invalid choice. Please enter a number between 0 and " + (instruction.OptionTextNodes.Count - 1) + ": ");
                    input = Console.ReadLine();
                }
                Console.WriteLine("=====================");
                var selectedActions = instruction.Blocks[choice];
                runtime.Enqueue(selectedActions, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing menu: {ex.Message}[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public virtual void Run(string filePath, string startLabel = "start")
        {
            var script = compiler.Compile(filePath);
            if (script == null)
            {
                Console.WriteLine("Failed to compile script.");
                return;
            }

            Runtime.ClearLabels();
            Runtime.ClearQueue();
            Runtime.Read(script);

            Runtime.Load(startLabel);

            while (Runtime.HasNext)
            {
                var instruction = Runtime.Pop();
                Execute(instruction, Runtime);
            }
        }
    }
}