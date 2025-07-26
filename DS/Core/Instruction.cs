using System;
using System.Linq;
using System.Collections.Generic;

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

    /// <summary>
    /// Executes the instruction.
    /// </summary>
    /// <param name="interpreter">The interpreter instance.</param>
    public abstract void Execute(Interpreter interpreter);
}

public class IR_Dialogue : IRInstruction
{
    public required string SpeakerName { get; init; }
    public required FStringNode TextNode { get; init; }
    public List<string> Tags { get; private set; } = [];
    public override void Execute(Interpreter interpreter)
    {
        interpreter.OnDialogue?.Invoke(interpreter, this);
    }
}

public class IR_Menu : IRInstruction
{
    public List<FStringNode> OptionTextNodes { get; private set; } = [];
    public List<List<IRInstruction>> Blocks { get; private set; } = [];
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
    public required string TargetLabel { get; init; }
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
    public required string TargetLabel { get; init; }
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
    public required string FunctionName { get; init; }
    public List<DSExpression> Arguments { get; private set; } = [];
    public override void Execute(Interpreter interpreter)
    {
        var args = Arguments.Select(arg => arg.Evaluate(interpreter)).ToArray();
        interpreter.Invoke(FunctionName, args);
    }
}

public class IR_Set : IRInstruction
{
    public required string VariableName { get; init; }
    public required string Symbol { get; init; } // Could be '=', '+=', '-=', etc.
    public required DSExpression Value { get; init; }
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
    public required DSExpression Condition { get; init; }
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