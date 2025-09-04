namespace DS.Core
{
    public class Executer
    {
        public void Execute(Runtime runtime, Statement statement)
        {
            switch (statement)
            {
                case Stmt_Dialogue dialogue:
                    OnDialogue?.Invoke(runtime, dialogue);
                    break;
                case Stmt_Menu menu:
                    OnMenu?.Invoke(runtime, menu);
                    break;
                case Stmt_Jump jump:
                    OnJump?.Invoke(runtime, jump);
                    break;
                case Stmt_Tour tour:
                    OnTour?.Invoke(runtime, tour);
                    break;
                case Stmt_Call call:
                    OnCall?.Invoke(runtime, call);
                    break;
                case Stmt_Assign set:
                    OnAssign?.Invoke(runtime, set);
                    break;
                case Stmt_If ifStmt:
                    OnIf?.Invoke(runtime, ifStmt);
                    break;
                default:
                    throw new NotSupportedException($"(Runtime Error) Unsupported instruction type: {statement.GetType().Name}");
            }
        }

        public Action<Runtime, Stmt_Dialogue> OnDialogue = (runtime, statement) => { throw new NotImplementedException("Ondialogue event must be implemented"); };

        public Action<Runtime, Stmt_Menu> OnMenu = (runtime, statement) => { throw new NotImplementedException("OnMenu event must be implemented"); };

        private readonly Action<Runtime, Stmt_Jump> OnJump = (runtime, statement) =>
        {
            try
            {
                var block = runtime.GetLabelBlock(statement.TargetLabel);
                runtime.ClearQueue();
                runtime.Enqueue(block.Instructions);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"(Runtime Error) Label '{statement.TargetLabel}' not found.[Ln {statement.LineNum}, Fp {statement.FilePath}]");
            }
        };

        private readonly Action<Runtime, Stmt_Tour> OnTour = (runtime, statement) =>
        {
            try
            {
                var block = runtime.GetLabelBlock(statement.TargetLabel);
                runtime.Enqueue(block.Instructions, true);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"(Runtime Error) Label '{statement.TargetLabel}' not found.[Ln {statement.LineNum}, Fp {statement.FilePath}]");
            }
        };

        private readonly Action<Runtime, Stmt_Call> OnCall = (runtime, statement) =>
        {
            try
            {
                var args = statement.Arguments.Select(arg => arg.Evaluate(runtime)).ToArray();
                runtime.Functions.Invoke(statement.FunctionName, args);
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException($"(Runtime Error) Function '{statement.FunctionName}' not found.[Ln {statement.LineNum}, Fp {statement.FilePath}]");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"(Runtime Error) Failed to call function '{statement.FunctionName}'. {ex.Message} [Ln {statement.LineNum}, Fp {statement.FilePath}]", ex);
            }
        };

        private readonly Action<Runtime, Stmt_Assign> OnAssign = (runtime, statement) =>
        {
            try
            {
                statement.Expression.Evaluate(runtime);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"(Runtime Error) Failed to evaluate assignment. {ex.Message} [Ln {statement.LineNum}, Fp {statement.FilePath}]", ex);
            }
        };

        private readonly Action<Runtime, Stmt_If> OnIf = (runtime, statement) =>
        {
            try
            {
                var conditionResult = statement.Condition.Evaluate(runtime);
                if (conditionResult == null || conditionResult is not bool)
                {
                    throw new InvalidOperationException($"(Runtime Error) Condition must evaluate to a boolean value.");
                }
                if ((bool)conditionResult)
                {
                    runtime.Enqueue(statement.TrueBranch, true);
                }
                else
                {
                    runtime.Enqueue(statement.FalseBranch, true);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"(Runtime Error) Failed to evaluate if condition. {ex.Message} [Ln {statement.LineNum}, Fp {statement.FilePath}]", ex);
            }
        };
    }
}