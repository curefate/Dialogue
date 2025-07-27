namespace DS.Core
{
    public class LabelBlock
    {
        public string LabelName { get; private set; }
        public string FileName { get; private set; }
        public List<IRInstruction> Instructions { get; private set; } = [];
        public LabelBlock(string labelName, string fileName)
        {
            LabelName = labelName;
            FileName = fileName;
        }
    }

    public abstract class IRInstruction
    {
        public required int LineNum { get; init; }
        public required string FilePath { get; init; }
    }

    public class IR_Dialogue : IRInstruction
    {
        public bool HasSpeaker => !string.IsNullOrEmpty(SpeakerName);
        public required string SpeakerName { get; init; }
        public required FStringNode TextNode { get; init; }
        public List<string> Tags { get; private set; } = [];
    }

    public class IR_Menu : IRInstruction
    {
        public List<FStringNode> OptionTextNodes { get; private set; } = [];
        public List<List<IRInstruction>> Blocks { get; private set; } = [];
        /* public override void Execute(Interpreter interpreter)
        {
            var choice = interpreter.OnMenu?.Invoke(this);
            if (choice.HasValue && choice.Value >= 0 && choice.Value < Blocks.Count)
            {
                var selectedActions = Blocks[choice.Value];
                for (int i = selectedActions.Count - 1; i >= 0; i--)
                {
                    var instruction = selectedActions[i];
                    interpreter.RunningQueue.AddFirst(instruction);
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid menu choice.[Ln {LineNum},Fp {FilePath}]");
            }
        } */
    }

    public class IR_Jump : IRInstruction
    {
        public required string TargetLabel { get; init; }
    }

    public class IR_Tour : IRInstruction
    {
        public required string TargetLabel { get; init; }
    }

    public class IR_Call : IRInstruction
    {
        public required string FunctionName { get; init; }
        public List<DSExpression> Arguments { get; private set; } = [];
    }

    public class IR_Set : IRInstruction
    {
        public required string VariableName { get; init; }
        public required string Symbol { get; init; } // Could be '=', '+=', '-=', etc.
        public required DSExpression Value { get; init; }
    }

    public class IR_If : IRInstruction
    {
        public required DSExpression Condition { get; init; }
        public List<IRInstruction> TrueBranch { get; private set; } = [];
        public List<IRInstruction> FalseBranch { get; private set; } = [];
    }
}