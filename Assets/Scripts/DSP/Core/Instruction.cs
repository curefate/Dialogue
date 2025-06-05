using UnityEngine;
using System;
using Assets.Scripts.DSP.Core;
using System.Collections.Generic;

public interface IIRInstruction
{
    /// <summary>
    /// Executes the instruction.
    /// </summary>
    /// <param name="interpreter">The interpreter instance.</param>
    void Execute(Interpreter interpreter);
}

// TODO IR得有嵌套结构，否则没法实现内联函数，也不一定，可以占位符不擦除继续执行
public class IR_Dialogue : IIRInstruction
{
    public bool IsSync { get; private set; }
    public string Speaker { get; private set; }
    public string Text { get; private set; }
    public string[] Tags { get; private set; }

    public void Execute(Interpreter interpreter)
    {
    }
}