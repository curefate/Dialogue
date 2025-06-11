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
    public List<string> Tags { get; private set; }
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Dialogue.Execute is not implemented yet.");
    }
}

public class IR_Menu : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public List<string> MenuOptions { get; set; } = new List<string>();
    public List<List<IIRInstruction>> MenuActions { get; set; } = new List<List<IIRInstruction>>();
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Menu.Execute is not implemented yet.");
    }
}

public class IR_Jump : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Jump.Execute is not implemented yet.");
    }
}

public class IR_Tour : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string TargetLabel { get; set; }
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Tour.Execute is not implemented yet.");
    }
}

public class IR_Call : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string FunctionName { get; set; }
    public List<Expression> Arguments { get; set; }
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Call.Execute is not implemented yet.");
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
        throw new NotImplementedException("IR_Set.Execute is not implemented yet.");
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
        throw new NotImplementedException("IR_If.Execute is not implemented yet.");
    }
}