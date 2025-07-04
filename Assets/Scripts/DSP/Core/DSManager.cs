using System.Collections.Generic;
using Assets.Scripts.DSP.Core;
using Mono.Cecil.Cil;
using UnityEngine;

public class DSManager : MonoBehaviour
{
    public TextAsset dialogueFile;

    [SerializeField]
    private Interpreter _interpreter;
    private readonly Compiler _compiler = new();

    void Awake()
    {
        if (_interpreter == null)
        {
            _interpreter = gameObject.AddComponent<Interpreter>();
        }
    }

    void Start()
    {
        var new_labels = _compiler.Compile(dialogueFile.text);
        foreach (var label in new_labels)
        {
            _interpreter.LabelBlocks.Add(label);
        }
        foreach (var label in _interpreter.LabelBlocks)
        {
            Debug.Log($"Label: {label.LabelName}, Instructions Count: {label.Instructions.Count}");
            label.Run(_interpreter);
        }
    }
}