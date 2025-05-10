using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DSP.Core
{
    // TODO, tree存root，operator子，variabel无子

    public class ExpressionTree
    {
        public ExpressionNode Root { get; set; }
        public int Depth { get; set; }
        public ExpressionTree(List<string> expr)
        {
            // TODO
        }
    }

    public class ExpressionNode
    {
        public OperatorType Operator { get; set; }
        public string Value { get; set; }

        // 一元运算符的子节点
        public ExpressionNode Child { get; set; }

        // 二元运算符的左右子节点
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }
        public enum OperatorType
        {
            Null,       // 叶节点(值节点)
            Not,        // 一元运算符
            And, Or,    // 逻辑二元运算符
            Equal, NotEqual, Greater, Less, GreaterEqual, LessEqual // 比较运算符
        }
    }
}