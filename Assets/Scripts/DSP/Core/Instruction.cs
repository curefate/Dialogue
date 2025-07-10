using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;

public class InstructionBlock
{
    public string LabelName { get; set; }
    public List<IIRInstruction> Instructions { get; private set; } = new List<IIRInstruction>();
    public void Run(Interpreter interpreter)
    {
        foreach (var instruction in Instructions)
        {
            instruction.Execute(interpreter);
        }
    }
}

public interface IIRInstruction
{
    /// <summary>
    /// Executes the instruction.
    /// </summary>
    /// <param name="interpreter">The interpreter instance.</param>
    void Execute(Interpreter interpreter);
}

public class IR_Dialogue : IIRInstruction
{
    public bool IsSync { get; set; }
    public string Speaker { get; set; }
    public DSExpression Text { get; set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public void Execute(Interpreter interpreter)
    {
        // TODO inner exprs
        interpreter.OnDialogue?.Invoke(this);
    }
}

public class IR_Menu : IIRInstruction
{
    public List<DSExpression> MenuOptions { get; set; } = new List<DSExpression>();
    public List<List<IIRInstruction>> MenuActions { get; set; } = new List<List<IIRInstruction>>();
    public void Execute(Interpreter interpreter)
    {
        interpreter.OnMenu?.Invoke(this);
        // TODO
    }
}

public class IR_Jump : IIRInstruction
{
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
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

public class IR_Tour : IIRInstruction
{
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
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

public class IR_Call : IIRInstruction
{
    public bool IsSync { get; set; }
    public string FunctionName { get; set; }
    public List<DSExpression> Arguments { get; set; } = new List<DSExpression>();
    public void Execute(Interpreter interpreter)
    {
        var args = Arguments.Select(arg => arg.Evaluate(interpreter)).ToArray();
        interpreter.Invoke(FunctionName, args);
    }
}

public class IR_Set : IIRInstruction
{
    public string VariableName { get; set; }
    public string Symbol { get; set; } // Could be '=', '+=', '-=', etc.
    public DSExpression Value { get; set; }
    public void Execute(Interpreter interpreter)
    {
        var evaluatedValue = Value.Evaluate(interpreter);
        switch (Symbol)
        {
            case "=":
                interpreter.SetVariable(VariableName[1..], evaluatedValue);
                return;
            // TODO ADD +=, -=, etc.
            default:
                throw new NotSupportedException($"Symbol '{Symbol}' is not supported.");
        }
    }
}

public class IR_If : IIRInstruction
{
    public DSExpression Condition { get; set; }
    public List<IIRInstruction> TrueBranch { get; private set; } = new List<IIRInstruction>();
    public List<IIRInstruction> FalseBranch { get; private set; } = new List<IIRInstruction>();
    public void Execute(Interpreter interpreter)
    {
        var conditionResult = Condition.Evaluate(interpreter);
        if (conditionResult == null || conditionResult is not bool)
        {
            throw new InvalidOperationException("Condition must evaluate to a boolean value.");
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