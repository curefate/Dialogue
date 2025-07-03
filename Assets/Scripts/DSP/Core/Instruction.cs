using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;
using System.Linq.Expressions;

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
    public string Text { get; set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public void Execute(Interpreter interpreter)
    {
        // TODO inner exprs and functions
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
        interpreter._runStack.Clear();
        var labelBlock = interpreter._labelBlocks.FirstOrDefault(l => l.LabelName == TargetLabel);
        if (labelBlock != null)
        {
            foreach (var instruction in labelBlock.Instructions)
            {
                interpreter._runStack.Push(instruction);
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
                interpreter._runStack.Push(instruction);
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
        var args = Arguments.Select(arg => interpreter.EvaluateExpression(arg).Value).ToArray();
        interpreter.Invoke(FunctionName, args);
    }
}

public class IR_Set : IIRInstruction
{
    public string VariableName { get; set; }
    public string Symbol { get; set; } // Could be '=', '+=', '-=', etc.
    public Expression Value { get; set; }
    public void Execute(Interpreter interpreter)
    {
        // Evaluate the expression and set the variable
        var evaluatedValue = interpreter.EvaluateExpression(Value);
        if (interpreter.ContainsVariable(VariableName))
        {
            switch (Symbol)
            {
                case "=":
                    interpreter.SetVariable(VariableName, evaluatedValue);
                    break;
                // TODO ADD +=, -=, etc.
                default:
                    throw new NotSupportedException($"Symbol '{Symbol}' is not supported.");
            }
        }
        else
        {
            interpreter.SetVariable(VariableName, evaluatedValue);
        }
    }
}

public class IR_If : IIRInstruction
{
    public Expression Condition { get; set; }
    public List<IIRInstruction> TrueBranch { get; private set; } = new List<IIRInstruction>();
    public List<IIRInstruction> FalseBranch { get; private set; } = new List<IIRInstruction>();
    public void Execute(Interpreter interpreter)
    {
        var conditionResult = interpreter.EvaluateExpression(Condition);
        if (conditionResult == null || conditionResult.Type != typeof(bool))
        {
            throw new InvalidOperationException("Condition must evaluate to a boolean value.");
        }
        if ((bool)conditionResult.Value)
        {
            foreach (var instruction in TrueBranch)
            {
                interpreter._runStack.Push(instruction);
            }
        }
        else
        {
            foreach (var instruction in FalseBranch)
            {
                interpreter._runStack.Push(instruction);
            }
        }
    }
}