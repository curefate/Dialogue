using UnityEngine;
using System;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using UnityEditor.Rendering.Canvas.ShaderGraph;

namespace Assets.Scripts.DSP.Core
{
    public class Interpreter : MonoBehaviour
    {
        /* private CommandSet Precompilation(string scriptContent)
        {
            throw new NotImplementedException();
        }
 */
        private string PreProcess(string scriptContent)
        {
            string ret = scriptContent.Replace("\t", "    ");
            ret += "\n";
            return ret;
        }
    }

    public class Visitor : DSParserBaseVisitor<bool>
    {

    }
}