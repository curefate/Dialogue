using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;
using System.Linq.Expressions;

public class LabelBlock
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
    public string Text { get; set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public void Execute(Interpreter interpreter)
    {
        interpreter.OnDialogue?.Invoke(this);
    }
}

public class IR_Menu : IIRInstruction
{
    public List<string> MenuOptions { get; set; } = new List<string>();
    public List<List<IIRInstruction>> MenuActions { get; set; } = new List<List<IIRInstruction>>();
    public void Execute(Interpreter interpreter)
    {
        interpreter.OnMenu?.Invoke(this);
    }
}

public class IR_Jump : IIRInstruction
{
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
    {
        interpreter._instructionStack.Clear();
        var labelBlock = interpreter._labelBlocks.FirstOrDefault(l => l.LabelName == TargetLabel);
        if (labelBlock != null)
        {
            foreach (var instruction in labelBlock.Instructions)
            {
                interpreter._instructionStack.Push(instruction);
            }
        }
    }
}

public class IR_Tour : IIRInstruction
{
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
    {
        var labelBlock = interpreter._labelBlocks.FirstOrDefault(l => l.LabelName == TargetLabel);
        if (labelBlock != null)
        {
            foreach (var instruction in labelBlock.Instructions)
            {
                interpreter._instructionStack.Push(instruction);
            }
        }
    }
}

public class IR_Call : IIRInstruction
{
    public string FunctionName { get; set; }
    public List<Expression> Arguments { get; set; } = new List<Expression>();
    public void Execute(Interpreter interpreter)
    {
        // expression to object

        // Invoke the function with the provided arguments

        // TODO
    }
}

public class IR_Set : IIRInstruction
{
    public string VariableName { get; set; }
    public string Symbol { get; set; } // Could be '=', '+=', '-=', etc.
    public Expression Value { get; set; }
    public void Execute(Interpreter interpreter)
    {
        // TODO
    }
}

public class IR_If : IIRInstruction
{
    public Expression Condition { get; set; }
    public List<IIRInstruction> TrueBranch { get; private set; } = new List<IIRInstruction>();
    public List<IIRInstruction> FalseBranch { get; private set; } = new List<IIRInstruction>();
    public void Execute(Interpreter interpreter)
    {
        // TODO
    }
}