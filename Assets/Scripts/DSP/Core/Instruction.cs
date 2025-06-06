using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;

public class LabelBlock
{
    public string Label { get; set; }
    public List<IIRInstruction> Instructions { get; set; } = new List<IIRInstruction>();
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
    public bool IsSync { get; private set; }
    public string Speaker { get; private set; }
    public string Text { get; private set; }
    public List<string> Tags { get; private set; }
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Dialogue.Execute is not implemented yet.");
    }
}

public class IR_Menu : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public List<string> MenuOptions { get; set; }
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
    public List<KeyValuePair<object, int>> Arguments { get; set; } // Pair<value, IToken.Type>
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Call.Execute is not implemented yet.");
    }
}

public class IR_Set : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public string VariableName { get; set; }
    public KeyValuePair<object, int> Value { get; set; } // Could be a primitive type or an expression
    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_Set.Execute is not implemented yet.");
    }
}

public class IR_If : IIRInstruction
{
    public string ErrorLog { get; set; } = string.Empty;
    public KeyValuePair<object, int> Condition { get; set; } // Could be a boolean expression or a variable
    public List<IIRInstruction> TrueBranch { get; set; } = new List<IIRInstruction>();
    public List<IIRInstruction> FalseBranch { get; set; } = new List<IIRInstruction>();

    public void Execute(Interpreter interpreter)
    {
        throw new NotImplementedException("IR_If.Execute is not implemented yet.");
    }
}