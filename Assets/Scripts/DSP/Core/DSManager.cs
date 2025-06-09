using System.Collections.Generic;
using Assets.Scripts.DSP.Core;
using UnityEngine;

public class DSManager : MonoBehaviour
{
    public Interpreter interpreter;
    private Compiler compiler;
    private List<LabelBlock> labelBlocks;
}