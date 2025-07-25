using System;
using System.Linq;
using System.Collections.Generic;

public class LabelBlock
{
    public string LabelName { get; private set; }
    public string FileName { get; private set; }
    public List<IRInstruction> Instructions { get; private set; } = new List<IRInstruction>();
    public LabelBlock(string labelName, string fileName)
    {
        LabelName = labelName;
        FileName = fileName;
    }
}

public abstract class IRInstruction
{
    public int LineNum { get; set; }
    public string FilePath { get; set; }

    /// <summary>
    /// Executes the instruction.
    /// </summary>
    /// <param name="interpreter">The interpreter instance.</param>
    public abstract void Execute(Interpreter interpreter);
}

public class IR_Dialogue : IRInstruction
{
    public string SpeakerName { get; set; }
    public FStringNode TextNode { get; set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public override void Execute(Interpreter interpreter)
    {
        interpreter.OnDialogue?.Invoke(interpreter, this);
    }
}

public class IR_Menu : IRInstruction
{
    public List<FStringNode> OptionTextNodes { get; set; } = new List<FStringNode>();
    public List<List<IRInstruction>> Blocks { get; set; } = new List<List<IRInstruction>>();
    public override void Execute(Interpreter interpreter)
    {
        var choice = interpreter.OnMenu?.Invoke(interpreter, this);
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
    }
}

public class IR_Jump : IRInstruction
{
    public string TargetLabel { get; set; }
    public override void Execute(Interpreter interpreter)
    {
        interpreter.RunningQueue.Clear();
        try
        {
            var labelBlock = interpreter.GetLabelBlock(TargetLabel);
            foreach (var instruction in labelBlock.Instructions)
            {
                interpreter.RunningQueue.AddLast(instruction);
            }
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Label '{TargetLabel}' not found.[Ln {LineNum},Fp {FilePath}]");
        }
    }
}

public class IR_Tour : IRInstruction
{
    public string TargetLabel { get; set; }
    public override void Execute(Interpreter interpreter)
    {
        try
        {
            var block = interpreter.GetLabelBlock(TargetLabel);
            for (int i = block.Instructions.Count - 1; i >= 0; i--)
            {
                var instruction = block.Instructions[i];
                interpreter.RunningQueue.AddFirst(instruction);
            }
        }
        catch (KeyNotFoundException)
        {
            throw new KeyNotFoundException($"Label '{TargetLabel}' not found.[Ln {LineNum},Fp {FilePath}]");
        }
    }
}

public class IR_Call : IRInstruction
{
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
                interpreter.SetVar(VariableName[1..], evaluatedValue);
                return;
            // TODO ADD +=, -=, etc.
            default:
                throw new NotSupportedException($"Symbol '{Symbol}' is not supported.[Ln {LineNum},Fp {FilePath}]");
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
            throw new InvalidOperationException($"Condition must evaluate to a boolean value.[Ln {LineNum},Fp {FilePath}]");
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