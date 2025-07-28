using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.Core
{
    public interface IIRExecuter
    {
        void Execute(IRInstruction instruction, RuntimeEnv runtime)
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

        void ExecuteDialogue(IR_Dialogue instruction, RuntimeEnv runtime);

        void ExecuteMenu(IR_Menu instruction, RuntimeEnv runtime);

        void ExecuteJump(IR_Jump instruction, RuntimeEnv runtime)
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

        void ExecuteTour(IR_Tour instruction, RuntimeEnv runtime)
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

        void ExecuteCall(IR_Call instruction, RuntimeEnv runtime)
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

        void ExecuteSet(IR_Set instruction, RuntimeEnv runtime)
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

        void ExecuteIf(IR_If instruction, RuntimeEnv runtime)
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
}