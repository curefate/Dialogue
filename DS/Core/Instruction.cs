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
        internal IR_Dialogue() { }
        public bool HasSpeaker => !string.IsNullOrEmpty(SpeakerName);
        public required string SpeakerName { get; init; }
        public required FStringNode TextNode { get; init; }
        public List<string> Tags { get; private set; } = [];
    }

    public class IR_Menu : IRInstruction
    {
        internal IR_Menu() { }
        public List<FStringNode> OptionTextNodes { get; private set; } = [];
        public List<List<IRInstruction>> Blocks { get; private set; } = [];
    }

    public class IR_Jump : IRInstruction
    {
        internal IR_Jump() { }
        public required string TargetLabel { get; init; }
    }

    public class IR_Tour : IRInstruction
    {
        internal IR_Tour() { }
        public required string TargetLabel { get; init; }
    }

    public class IR_Call : IRInstruction
    {
        internal IR_Call() { }
        public required string FunctionName { get; init; }
        public List<DSExpression> Arguments { get; private set; } = [];
    }

    public class IR_Set : IRInstruction
    {
        internal IR_Set() { }
        public required string VariableName { get; init; }
        public required string Symbol { get; init; } // Could be '=', '+=', '-=', etc.
        public required DSExpression Value { get; init; }
    }

    public class IR_If : IRInstruction
    {
        internal IR_If() { }
        public required DSExpression Condition { get; init; }
        public List<IRInstruction> TrueBranch { get; private set; } = [];
        public List<IRInstruction> FalseBranch { get; private set; } = [];
    }
}