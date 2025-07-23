using Assets.Scripts.DSP.Core;
using UnityEngine;

public class test2 : MonoBehaviour
{
    public Interpreter interpreter;
    public Compiler compiler;

    void Awake()
    {
        if (interpreter == null)
        {
            interpreter ??= new Interpreter();
            interpreter.OnDialogue += (dialogue) =>
            {
                Debug.Log($"{dialogue.Speaker ?? "null"}({dialogue.IsSync}): {(string)dialogue.Text.Evaluate(interpreter)} ({string.Join(", ", dialogue.Tags)})");
            };
            interpreter.AddFunction("Test", Test);
            interpreter.AddFunction<int, int>("Test2", Test2);
            interpreter.AddFunction<string, string>("Test3", Test3);
            interpreter.AddFunction<bool>("Test4", Test4);
            interpreter.AddFunction<int, int, int>("TestAdd", TestAdd);
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

    public void Test4(bool a)
    {
        Debug.Log($"Test4 function called with parameter: {a}");
    }

    public int TestAdd(int a, int b)
    {
        return a + b;
    }

    void Start()
    {
        interpreter.Load(compiler.Compile("Assets\\Resources\\test2.txt"));
        Debug.Log("=======================================================");
        interpreter.Run();
    }
}
