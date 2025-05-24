using Antlr4.Runtime;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;

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
        visitor.Visit(tree);
    }
}

// 简单访问器（继承自生成的DialogueScriptBaseVisitor）
public class BasicDialogueVisitor : DSParserBaseVisitor<object>
{
    public override object VisitDialogue_stmt(DSParser.Dialogue_stmtContext context)
    {
        var text = context.text.Text.Trim('"');
        var speaker = context.ID()?.GetText() ?? "Null";
        var tags = context._tags;
        string log = $"说话者: {speaker}, 内容: {text}";
        foreach (var tag in tags)
        {
            log += $"标签: {tag.Text}";
        }
        Debug.Log(log);

        return null;
    }

    public override object VisitMenu_stmt([NotNull] DSParser.Menu_stmtContext context)
    {
        Debug.Log("菜单选项");
        var intro = context.intro;
        if (intro != null)
            VisitDialogue_stmt(intro);
        var options = context._option;
        foreach (var option in options)
        {
            var text = option.text.Text.Trim('"');
            Debug.Log("选项: " + text);
            var block = option.block();
            Visit(block);
        }
        Debug.Log("菜单结束");
        return null;
    }

    public override object VisitLabel_decl([NotNull] DSParser.Label_declContext context)
    {
        var labelName = context.label.Text;
        Debug.Log($"标签: {labelName}");
        return base.VisitLabel_decl(context);
    }

    public override object VisitJump_stmt([NotNull] DSParser.Jump_stmtContext context)
    {
        var labelName = context.label.Text;
        Debug.Log($"跳转到标签: {labelName}");
        return base.VisitJump_stmt(context);
    }
}