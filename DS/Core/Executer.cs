namespace DS.Core
{
    public abstract class Executer
    {
        public virtual void Execute(Statement instruction, Runtime runtime)
        {
            switch (instruction)
            {
                case Stmt_Dialogue dialogue:
                    ExecuteDialogue(dialogue, runtime);
                    break;
                case Stmt_Menu menu:
                    ExecuteMenu(menu, runtime);
                    break;
                case Stmt_Jump jump:
                    ExecuteJump(jump, runtime);
                    break;
                case Stmt_Tour tour:
                    ExecuteTour(tour, runtime);
                    break;
                case Stmt_Call call:
                    ExecuteCall(call, runtime);
                    break;
                case Stmt_Assign set:
                    ExecuteAssign(set, runtime);
                    break;
                case Stmt_If ifInstruction:
                    ExecuteIf(ifInstruction, runtime);
                    break;
                default:
                    throw new NotSupportedException($"(Runtime Error) Unsupported instruction type: {instruction.GetType().Name}");
            }
        }

        public abstract void ExecuteDialogue(Stmt_Dialogue instruction, Runtime runtime);

        public abstract void ExecuteMenu(Stmt_Menu instruction, Runtime runtime);

        public virtual void ExecuteJump(Stmt_Jump instruction, Runtime runtime)
        {
            try
            {
                var block = runtime.GetLabelBlock(instruction.TargetLabel);
                runtime.ClearQueue();
                runtime.Enqueue(block.Instructions);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"(Runtime Error) Label '{instruction.TargetLabel}' not found.[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public virtual void ExecuteTour(Stmt_Tour instruction, Runtime runtime)
        {
            try
            {
                var block = runtime.GetLabelBlock(instruction.TargetLabel);
                runtime.Enqueue(block.Instructions, true);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"(Runtime Error) Label '{instruction.TargetLabel}' not found.[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
        }

        public virtual void ExecuteCall(Stmt_Call instruction, Runtime runtime)
        {
            try
            {
                var args = instruction.Arguments.Select(arg => arg.Evaluate(runtime)).ToArray();
                runtime.Functions.Invoke(instruction.FunctionName, args);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"(Runtime Error) Function '{instruction.FunctionName}' not found.[Ln {instruction.LineNum}, Fp {instruction.FilePath}]");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"(Runtime Error) Failed to call function '{instruction.FunctionName}'. {ex.Message} [Ln {instruction.LineNum}, Fp {instruction.FilePath}]", ex);
            }
        }

        public virtual void ExecuteAssign(Stmt_Assign instruction, Runtime runtime)
        {
            try
            {
                instruction.Expression.Evaluate(runtime);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"(Runtime Error) Failed to evaluate assignment. {ex.Message} [Ln {instruction.LineNum}, Fp {instruction.FilePath}]", ex);
            }

        }

        public virtual void ExecuteIf(Stmt_If instruction, Runtime runtime)
        {
            try
            {
                var conditionResult = instruction.Condition.Evaluate(runtime);
                if (conditionResult == null || conditionResult is not bool)
                {
                    throw new InvalidOperationException($"(Runtime Error) Condition must evaluate to a boolean value.");
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
                throw new InvalidOperationException($"(Runtime Error) Failed to evaluate if condition. {ex.Message} [Ln {instruction.LineNum}, Fp {instruction.FilePath}]", ex);
            }
        }
    }
}