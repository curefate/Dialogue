using Antlr4.Runtime;
using Assets.Scripts.DSP.Core;
using UnityEngine;

public class test2 : MonoBehaviour
{
    public TextAsset dialogueFile;
    public Interpreter interpreter;
    public Compiler compiler;

    void Awake()
    {
        if (interpreter == null)
        {
            interpreter = gameObject.AddComponent<Interpreter>();
            interpreter.OnDialogue += (dialogue) =>
            {
                Debug.Log($"{dialogue.Speaker ?? "null"}({dialogue.IsSync}): {dialogue.Text}");
            };
            interpreter.AddFunction("Test", Test);
            interpreter.AddFunction<int, int>("Test2", Test2);
            interpreter.AddFunction<string, string>("Test3", Test3);
        }
        compiler ??= new Compiler();
    }

    public void Test()
    {
        Debug.Log("Test function called.");
    }

    public void Test2(int a, int b)
    {
        Debug.Log($"Test2 function called with parameters: {a}, {b}");
    }

    public void Test3(string a, string b)
    {
        Debug.Log($"Test3 function called with parameters: {a}, {b}");
    }

    void Start()
    {
        if (dialogueFile == null)
        {
            Debug.LogError("Dialogue file is not assigned.");
            return;
        }

        var labels = compiler.Compile(dialogueFile.text);
        foreach (var label in labels)
        {
            interpreter.LabelBlocks.Add(label);
            Debug.Log($"Label({label.Instructions.Count}): {label.LabelName}");
        }
        Debug.Log("=======================================================");

        interpreter.Run();
        while (interpreter.RunningQueue.Count > 0)
        {
            interpreter.Next();
        }
    }
}
