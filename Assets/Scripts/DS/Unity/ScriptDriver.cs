using UnityEngine;
using DS.Core;

public class ScriptDriver : MonoBehaviour, IIRExecuter
{
    public string FilePath;
    public string StartLabel = "start";
    public bool ClickToContinue = true;
    public ChatBubble DialogueBubble;
    public ChatBubble NarratorBubble;
    public GameObject OptionBubblePrefab;

    public RuntimeEnv Runtime { get; private set; } = new();
    protected readonly Compiler compiler = new();

    public void ExecuteDialogue(IR_Dialogue instruction, RuntimeEnv runtime)
    {
        
    }

    public void ExecuteMenu(IR_Menu instruction, RuntimeEnv runtime)
    {

    }

    public void Run()
    {
        var script = compiler.Compile(FilePath);
        if (script == null)
        {
            Debug.LogError("Failed to compile script.");
            return;
        }

        try
        {
            var block = Runtime.GetLabelBlock(StartLabel);
            Runtime.Enqueue(block.Instructions, true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error running script: {ex.Message}");
        }
    }
}
