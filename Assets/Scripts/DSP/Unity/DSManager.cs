using UnityEngine;
using Assets.Scripts.DSP.Core;

public class DSManager : MonoBehaviour
{
    public Interpreter interpreter;
    public Compiler compiler;

    void Awake()
    {
        interpreter ??= new Interpreter();
        // TODO
        // interpreter.OnDialogue += (dialogue) =>
        // interpreter.AddFunction
        compiler ??= new Compiler();
    }
}