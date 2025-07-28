using System.Collections.Generic;

namespace DS.Core
{
    public class LabelBlock
    {
        public string LabelName { get; private set; }
        public string FileName { get; private set; }
        public List<IRInstruction> Instructions { get; private set; } = new();
        public LabelBlock(string labelName, string fileName)
        {
            LabelName = labelName;
            FileName = fileName;
        }
    }

    public abstract class IRInstruction
    {
        public int LineNum { get; init; }
        public string FilePath { get; init; }
    }

    public class IR_Dialogue : IRInstruction
    {
        internal IR_Dialogue() { }
        public bool HasSpeaker => !string.IsNullOrEmpty(SpeakerName);
        public string SpeakerName { get; init; }
        public FStringNode TextNode { get; init; }
        public List<string> Tags { get; private set; } = new();
    }

    public class IR_Menu : IRInstruction
    {
        internal IR_Menu() { }
        public List<FStringNode> OptionTextNodes { get; private set; } = new();
        public List<List<IRInstruction>> Blocks { get; private set; } = new();
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
        internal IR_Jump() { }
        public string TargetLabel { get; init; }
    }

    public class IR_Tour : IRInstruction
    {
        internal IR_Tour() { }
        public string TargetLabel { get; init; }
    }

    public class IR_Call : IRInstruction
    {
        internal IR_Call() { }
        public string FunctionName { get; init; }
        public List<DSExpression> Arguments { get; private set; } = new();
    }

    public class IR_Set : IRInstruction
    {
        internal IR_Set() { }
        public string VariableName { get; init; }
        public string Symbol { get; init; } // Could be '=', '+=', '-=', etc.
        public DSExpression Value { get; init; }
    }

    public class IR_If : IRInstruction
    {
        internal IR_If() { }
        public DSExpression Condition { get; init; }
        public List<IRInstruction> TrueBranch { get; private set; } = new();
        public List<IRInstruction> FalseBranch { get; private set; } = new();
    }
}