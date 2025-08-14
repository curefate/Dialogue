namespace DS.Core
{
    public class LabelBlock
    {
        public string LabelName { get; private set; }
        public string FileName { get; private set; }
        public List<Statement> Instructions { get; private set; } = [];
        public LabelBlock(string labelName, string fileName)
        {
            LabelName = labelName;
            FileName = fileName;
        }
    }

    public abstract class Statement
    {
        public required int LineNum { get; init; }
        public required string FilePath { get; init; }
    }

    public class Stmt_Dialogue : Statement
    {
        internal Stmt_Dialogue() { }
        public bool HasSpeaker => !string.IsNullOrEmpty(SpeakerName);
        public required string SpeakerName { get; init; }
        public required FStringNode TextNode { get; init; }
        public List<string> Tags { get; private set; } = [];

        public override string ToString()
        {
            return $"{(HasSpeaker ? SpeakerName + ": " : "")}{TextNode}";
        }
    }

    public class Stmt_Menu : Statement
    {
        internal Stmt_Menu() { }
        public List<FStringNode> OptionTextNodes { get; private set; } = [];
        public List<List<Statement>> Blocks { get; private set; } = [];

        public override string ToString()
        {
            System.Text.StringBuilder sb = new();
            for (int i = 0; i < OptionTextNodes.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {OptionTextNodes[i]}:");
                foreach (var instr in Blocks[i])
                {
                    sb.AppendLine($"    {instr}");
                }
            }
            return sb.ToString();
        }
    }

    public class Stmt_Jump : Statement
    {
        internal Stmt_Jump() { }
        public required string TargetLabel { get; init; }

        public override string ToString()
        {
            return $"Jump to {TargetLabel}";
        }
    }

    public class Stmt_Tour : Statement
    {
        internal Stmt_Tour() { }
        public required string TargetLabel { get; init; }

        public override string ToString()
        {
            return $"Tour to {TargetLabel}";
        }
    }

    public class Stmt_Call : Statement
    {
        internal Stmt_Call() { }
        public required string FunctionName { get; init; }
        public List<Expression> Arguments { get; private set; } = [];

        public override string ToString()
        {
            return $"Call {FunctionName}({string.Join(", ", Arguments)})";
        }
    }

    public class Stmt_Assign : Statement
    {
        internal Stmt_Assign() { }
        public required Expression Expression { get; init; }

        public override string ToString()
        {
            return Expression.ToString();
        }
    }

    public class Stmt_If : Statement
    {
        internal Stmt_If() { }
        public required Expression Condition { get; init; }
        public List<Statement> TrueBranch { get; private set; } = [];
        public List<Statement> FalseBranch { get; private set; } = [];

        public override string ToString()
        {
            System.Text.StringBuilder sb = new();
            sb.AppendLine($"If ({Condition}):");
            foreach (var instr in TrueBranch)
            {
                sb.AppendLine($"    {instr}");
            }
            if (FalseBranch.Count > 0)
            {
                sb.AppendLine("Else:");
                foreach (var instr in FalseBranch)
                {
                    sb.AppendLine($"    {instr}");
                }
            }
            return sb.ToString();
        }
    }
}