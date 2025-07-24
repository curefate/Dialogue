using UnityEngine;
using Assets.Scripts.DS.Core;
using System.Collections.Generic;

public class DSManager : MonoBehaviour
{
    public Interpreter Interpreter;
    public Compiler Compiler;
    [SerializeField]
    private List<string> ScriptFilePaths;

    void Awake()
    {
        Interpreter ??= new Interpreter();
        // TODO
        // interpreter.OnDialogue += (dialogue) =>
        // interpreter.AddFunction
        Compiler ??= new Compiler();
    }

    void Start()
    {
        if (ScriptFilePaths != null && ScriptFilePaths.Count > 0)
        {
            foreach (var filePath in ScriptFilePaths)
            {
                var script = Compiler.Compile(filePath);
                Interpreter.Load(script);
            }
        }
        Interpreter.Run();
    }

    public void Load(string filePath)
    {
        var script = Compiler.Compile(filePath);
        Interpreter.Load(script);
        ScriptFilePaths.Add(filePath);
    }

    public void Run(string label = "start")
    {
        if (Interpreter.IsRunning)
        {
            Debug.LogWarning("Interpreter is already running.");
            return;
        }

        Interpreter.Run(label);
    }
}