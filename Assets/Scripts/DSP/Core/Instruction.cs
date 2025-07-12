using System;
using System.Linq;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;

public class InstructionBlock
{
    public string LabelName { get; private set; }
    public string FileName { get; private set; }
    public List<IRInstruction> Instructions { get; private set; } = new List<IRInstruction>();
    public InstructionBlock(string labelName, string fileName)
    {
        LabelName = labelName;
        FileName = fileName;
    }
    public void Run(Interpreter interpreter)
    {
        foreach (var instruction in Instructions)
        {
            instruction.Execute(interpreter);
        }
    }
}

public abstract class IRInstruction
{
    public int Line { get; set; }
    public string File { get; set; }

    /// <summary>
    /// Executes the instruction.
    /// </summary>
    /// <param name="interpreter">The interpreter instance.</param>
    public abstract void Execute(Interpreter interpreter);
}

public class IR_Dialogue : IRInstruction
{
    public bool IsSync { get; set; }
    public string Speaker { get; set; }
    public FStringNode Text { get; set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public override void Execute(Interpreter interpreter)
    {
        interpreter.OnDialogue?.Invoke(this);
    }
}

public class IR_Menu : IRInstruction
{
    public List<FStringNode> MenuOptions { get; set; } = new List<FStringNode>();
    public List<List<IRInstruction>> MenuActions { get; set; } = new List<List<IRInstruction>>();
    public override void Execute(Interpreter interpreter)
    {
        var choice = interpreter.OnMenu?.Invoke(this);
        if (choice.HasValue && choice.Value >= 0 && choice.Value < MenuActions.Count)
        {
            var selectedActions = MenuActions[choice.Value];
            for (int i = selectedActions.Count - 1; i >= 0; i--)
            {
                var instruction = selectedActions[i];
                interpreter.RunningQueue.AddFirst(instruction);
            }
        }
        else
        {
            throw new InvalidOperationException($"Invalid menu choice.[Ln {Line},Fl {File}]");
        }
    }
}

public class IR_Jump : IRInstruction
{
    public string TargetLabel { get; set; }
    public override void Execute(Interpreter interpreter)
    {
        interpreter.RunningQueue.Clear();
        var labelBlock = interpreter.LabelBlocks.FirstOrDefault(l => l.LabelName == TargetLabel);
        if (labelBlock != null)
        {
            foreach (var instruction in labelBlock.Instructions)
            {
                interpreter.RunningQueue.AddLast(instruction);
            }
        }
    }
}

public class IR_Tour : IRInstruction
{
    public string TargetLabel { get; set; }
    public override void Execute(Interpreter interpreter)
    {
        var block = interpreter.LabelBlocks.FirstOrDefault(l => l.LabelName == TargetLabel);
        if (block != null)
        {
            for (int i = block.Instructions.Count - 1; i >= 0; i--)
            {
                var instruction = block.Instructions[i];
                interpreter.RunningQueue.AddFirst(instruction);
            }
        }
    }
}

public class IR_Call : IRInstruction
{
    public bool IsSync { get; set; }
    public string FunctionName { get; set; }
    public List<DSExpression> Arguments { get; set; } = new List<DSExpression>();
    public override void Execute(Interpreter interpreter)
    {
        var args = Arguments.Select(arg => arg.Evaluate(interpreter)).ToArray();
        interpreter.Invoke(FunctionName, args);
    }
}

public class IR_Set : IRInstruction
{
    public string VariableName { get; set; }
    public string Symbol { get; set; } // Could be '=', '+=', '-=', etc.
    public DSExpression Value { get; set; }
    public override void Execute(Interpreter interpreter)
    {
        var evaluatedValue = Value.Evaluate(interpreter);
        switch (Symbol)
        {
            case "=":
                interpreter.SetVariable(VariableName[1..], evaluatedValue);
                return;
            // TODO ADD +=, -=, etc.
            default:
                throw new NotSupportedException($"Symbol '{Symbol}' is not supported.[Ln {Line},Fl {File}]");
        }
    }
}

public class IR_If : IRInstruction
{
    public DSExpression Condition { get; set; }
    public List<IRInstruction> TrueBranch { get; private set; } = new List<IRInstruction>();
    public List<IRInstruction> FalseBranch { get; private set; } = new List<IRInstruction>();
    public override void Execute(Interpreter interpreter)
    {
        var conditionResult = Condition.Evaluate(interpreter);
        if (conditionResult == null || conditionResult is not bool)
        {
            throw new InvalidOperationException($"Condition must evaluate to a boolean value.[Ln {Line},Fl {File}]");
        }
        if ((bool)conditionResult)
        {
            for (int i = TrueBranch.Count - 1; i >= 0; i--)
            {
                var instruction = TrueBranch[i];
                interpreter.RunningQueue.AddFirst(instruction);
            }
        }
        else
        {
            for (int i = FalseBranch.Count - 1; i >= 0; i--)
            {
                var instruction = FalseBranch[i];
                interpreter.RunningQueue.AddFirst(instruction);
            }
        }
    }
}