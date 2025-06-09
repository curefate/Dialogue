using Antlr4.Runtime;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using UnityEditor.Build.Content;

public class DialogueRunner : MonoBehaviour
{
    public TextAsset dialogueFile; // Unity 中拖入的对话脚本文件

    void Start()
    {
        // 1. 创建输入流
        var inputStream = new AntlrInputStream(dialogueFile.text);

        // 2. 创建词法分析器
        var lexer = new DSLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);

        tokens.Fill();
        /* foreach (var token in tokens.GetTokens())
        {
            string channel = token.Channel == Lexer.Hidden ? "HIDDEN" : "DEFAULT";
            Debug.Log($"[{channel}] {lexer.Vocabulary.GetSymbolicName(token.Type)}: {token.Text}: {token.Line}");
        } */
        Debug.Log("=======================================================================");

        // 3. 创建语法分析器
        var parser = new DSParser(tokens);

        // 4. 解析并获取语法树（从program规则开始）
        var tree = parser.program();

        // 5. 遍历语法树（示例：打印所有对话文本）
        var visitor = new BasicDialogueVisitor();
        Debug.Log(visitor.Visit(tree));
    }
}

// 简单访问器（继承自生成的DialogueScriptBaseVisitor）
public class BasicDialogueVisitor : DSParserBaseVisitor<int>
{
    public override int VisitDialogue_stmt(DSParser.Dialogue_stmtContext context)
    {
        bool isSync = context.SYNC() != null;
        var text = context.text.Text.Trim('"');
        var speaker = context.ID()?.GetText() ?? "Null";
        var tags = context._tags;
        string log = $"{isSync} 说话者: {speaker}, 内容: {text}";
        foreach (var tag in tags)
        {
            log += $"标签: {tag.Text}";
        }
        Debug.Log(log);

        return 0;
    }

    public override int VisitMenu_stmt([NotNull] DSParser.Menu_stmtContext context)
    {
        Debug.Log("访问菜单语句");
        var options = context._options;
        foreach (var option in options)
        {
            var optionText = option.text.Text.Trim('"');
            Debug.Log($"菜单选项: {optionText}");
            Visit(option.block());
        }
        Debug.Log("菜单语句结束");
        return 1;
    }

    public override int VisitJump_stmt([NotNull] DSParser.Jump_stmtContext context)
    {
        var labelName = context.label.Text;
        Debug.Log($"跳转到标签: {labelName}");
        return 2;
    }

    public override int VisitTour_stmt([NotNull] DSParser.Tour_stmtContext context)
    {
        var tourName = context.label.Text;
        Debug.Log($"巡游到标签: {tourName}");
        return 3;
    }

    public override int VisitCall_stmt([NotNull] DSParser.Call_stmtContext context)
    {
        var log = $"调用 {context.func_name.Text}";
        var args = context._args;
        foreach (var arg in args)
        {
            log += $" 参数: {arg.GetText()}";
        }
        Debug.Log(log);
        return 4;
    }

    public override int VisitSet_stmt([NotNull] DSParser.Set_stmtContext context)
    {
        var v = context.VARIABLE().GetText();
        var value = context.expression().GetText();
        Debug.Log($"赋值: {v} = {value}");
        return 5;
    }

    public override int VisitIf_stmt([NotNull] DSParser.If_stmtContext context)
    {
        return 6;
    }
}