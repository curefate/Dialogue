using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;
using System.Linq.Expressions;

public class LabelBlock
{
    public string Label { get; set; }
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
    string ErrorLog { get; set; }
    /// <summary>
    /// Executes the instruction.
    /// </summary>
    /// <param name="interpreter">The interpreter instance.</param>
    void Execute(Interpreter interpreter);
}

public class IR_Dialogue : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public bool IsSync { get; set; }
    public string Speaker { get; set; }
    public string Text { get; set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_Dialogue: {Text} by {Speaker} (Sync: {IsSync}) (Tags: {string.Join(", ", Tags)})");
    }
}

public class IR_Menu : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public List<string> MenuOptions { get; set; } = new List<string>();
    public List<List<IIRInstruction>> MenuActions { get; set; } = new List<List<IIRInstruction>>();
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_Menu with options: {string.Join(", ", MenuOptions)}");
        for (int i = 0; i < MenuOptions.Count; i++)
        {
            Debug.Log($"Option {i + 1}: {MenuOptions[i]}");
            if (MenuActions.Count > i)
            {
                foreach (var action in MenuActions[i])
                {
                    action.Execute(interpreter);
                }
            }
        }
    }
}

public class IR_Jump : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_Jump to label: {TargetLabel}");
    }
}

public class IR_Tour : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_Tour to label: {TargetLabel}");
    }
}

public class IR_Call : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string FunctionName { get; set; }
    public List<Expression> Arguments { get; set; } = new List<Expression>();
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_Call to function: {FunctionName} with arguments: {string.Join(", ", Arguments.Select(a => a.ToString()))}");
    }
}

public class IR_Set : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string VariableName { get; set; }
    public string Symbol { get; set; } // Could be '=', '+=', '-=', etc.
    public Expression Value { get; set; }
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_Set: {VariableName} {Symbol} {Value}");
    }
}

public class IR_If : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public Expression Condition { get; set; }
    public List<IIRInstruction> TrueBranch { get; private set; } = new List<IIRInstruction>();
    public List<IIRInstruction> FalseBranch { get; private set; } = new List<IIRInstruction>();
    public void Execute(Interpreter interpreter)
    {
        Debug.Log($"Executing IR_If with condition: {Condition}");
        Debug.Log("True Branch:");
        foreach (var instruction in TrueBranch)
        {
            instruction.Execute(interpreter);
        }
        Debug.Log("False Branch:");
        foreach (var instruction in FalseBranch)
        {
            instruction.Execute(interpreter);
        }
    }
}