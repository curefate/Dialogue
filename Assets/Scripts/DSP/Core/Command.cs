using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts.DSP.Core
{
    //--------------------------------------------------------------------------
    public class CommandSet
    {
        public List<LabelCommand> CmdLines { get; } = new List<LabelCommand>();
        public Dictionary<string, int> LabelMap { get; } = new Dictionary<string, int>();
    }


    //--------------------------------------------------------------------------
    // base class
    public abstract class Command
    {
        public abstract CommandType Type { get; }
        public int LineNum { get; set; } // 此处的行号指的是脚本中的行号，用于debug用
    }

    public abstract class SynchronizableCommand : Command
    {
        public bool IsSync { get; set; } = false;
    }


    //--------------------------------------------------------------------------
    // command class
    public class LabelCommand : Command
    {
        public string Label { get; set; }
        public override CommandType Type => CommandType.Label;
    }

    public class DialogueCommand : SynchronizableCommand
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public List<Command> InnerCommands { get; } = new List<Command>();
        public List<ExpressionTree> InnerExpressions { get; } = new List<ExpressionTree>();
        public List<string> Tags { get; } = new List<string>();
        public override CommandType Type => CommandType.Dialogue;
    }

    public class JumpCommand : Command
    {
        public string Target { get; set; }
        public override CommandType Type => CommandType.Jump;
    }

    public class MenuCommand : Command
    {
        public Command Intro { get; set; }
        public List<string> Option { get; } = new List<string>();
        public List<List<Command>> Block { get; } = new List<List<Command>>();
        public override CommandType Type => CommandType.Menu;
    }

    public class JudgeCommand : Command
    {
        public ExpressionTree Condition { get; set; }
        public List<Command> TBlock { get; } = new List<Command>();
        public List<Command> FBlock { get; } = new List<Command>();
        public override CommandType Type => CommandType.Judge;
    }

    public class WhileCommand : Command
    {
        public ExpressionTree Condition { get; set; }
        public List<Command> Block { get; } = new List<Command>();
        public override CommandType Type => CommandType.While;
    }

    public class MatchCommand : Command
    {
        public string Target { get; set; }
        public Dictionary<string, List<Command>> Cases { get; } = new Dictionary<string, List<Command>>();
        public override CommandType Type => CommandType.Match;
    }

    public class CallCommand : SynchronizableCommand
    {
        public List<string> Args { get; set; } = new List<string>();
        public override CommandType Type => CommandType.Call;
    }

    public class EndCommand : Command
    {
        public override CommandType Type => CommandType.End;
    }


    //--------------------------------------------------------------------------
    public enum CommandType
    {
        Null,
        Label,
        Dialogue,
        Jump,
        Menu,
        Judge,
        While,
        Match,
        Call,
        End,
    }
}