using UnityEngine;
using Assets.Scripts.DS.Core;
using System.Collections.Generic;

public class SimpleScriptManager : MonoBehaviour
{
    public BubblePusher BubblePusher;
    public Interpreter Interpreter;
    public Compiler Compiler;
    [ContextMenuItem("Run", "Run")]
    public string StartFrom = "start";
    [SerializeField]
    private List<string> ScriptFilePaths;


    void Awake()
    {
        if (BubblePusher == null)
        {
            BubblePusher = this.gameObject.GetComponent<BubblePusher>();
            if (BubblePusher == null)
            {
                Debug.LogError("BubblePusher not found in the scene.");
            }
        }
        Interpreter ??= new Interpreter();
        Interpreter.OnDialogue = BubblePusher.PushDialogue; // Set the dialogue handler to BubblePusher
        Interpreter.OnMenu = BubblePusher.PushMenu; // Set the menu handler to BubblePusher
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
    }

    public void Load(string filePath)
    {
        var script = Compiler.Compile(filePath);
        Interpreter.Load(script);
        ScriptFilePaths.Add(filePath);
    }

    public void Run()
    {
        if (Interpreter.IsRunning)
        {
            Debug.LogWarning("Interpreter is already running.");
            return;
        }
        Interpreter.Run(StartFrom);
    }
}