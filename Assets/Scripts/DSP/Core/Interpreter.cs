using UnityEngine;
using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using UnityEditor.Rendering.Canvas.ShaderGraph;
using Mono.Cecil.Cil;

namespace Assets.Scripts.DSP.Core
{
    public class Interpreter : MonoBehaviour
    {
        public Action<IR_Dialogue> OnDialogue;
        

        /* private List<Instruction> Precompilation(string ctx)
        {
            throw new NotImplementedException("Precompilation is not implemented yet.");
        }

        private string PreProcess(string scriptContent)
        {
            string ret = scriptContent.Replace("\t", "    ");
            ret += "\n";
            return ret;
        } */
    }
}