using System.Collections.Generic;
using Assets.Scripts.DSP.Core;
using UnityEngine;

public class DSManager : MonoBehaviour
{
    public TextAsset dialogueFile;

    [SerializeField]
    private Interpreter interpreter;
    private readonly Compiler compiler = new();
    private readonly List<LabelBlock> labelBlocks = new();

    void Awake()
    {
        if (interpreter == null)
        {
            interpreter = gameObject.AddComponent<Interpreter>();
        }
    }

    void Start()
    {
        var new_labels = compiler.Compile(dialogueFile.text);
        foreach (var label in new_labels)
        {
            labelBlocks.Add(label);
        }
        foreach (var label in labelBlocks)
        {
            Debug.Log($"Label: {label.Label}, Instructions Count: {label.Instructions.Count}");
            label.Run(interpreter);
        }
    }

    private void SetInterpreter(Interpreter newInterpreter)
    {
        if (interpreter != null)
        {
            Destroy(interpreter);
        }
        interpreter = newInterpreter;
    }
}