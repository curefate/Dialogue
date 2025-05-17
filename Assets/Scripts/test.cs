using Antlr4.Runtime;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Antlr4.Runtime.Misc;

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

        // 3. 创建语法分析器
        // var parser = new DSParser(tokens);

        // 4. 解析并获取语法树（从program规则开始）
        // var tree = parser.program();

        // 5. 遍历语法树（示例：打印所有对话文本）
        // var visitor = new BasicDialogueVisitor();
        // visitor.Visit(tree);
    }
}

// 简单访问器（继承自生成的DialogueScriptBaseVisitor）
public class BasicDialogueVisitor : DSParserBaseVisitor<object>
{
    public override object VisitDialogue_stmt(DSParser.Dialogue_stmtContext context)
    {
        Debug.Log(context);
        var text = context.text.Text.Trim('"');
        var speaker = context.ID()?.GetText() ?? "Null";
        var tags = context._tags;
        Debug.Log($"说话者: {speaker}, 内容: {text}");
        foreach (var tag in tags)
        {
            Debug.Log($"标签: {tag.Text}");
        }

        return null;
    }

    public override object VisitMenu_stmt([NotNull] DSParser.Menu_stmtContext context)
    {
        Debug.Log("1");
        var intro = context.intro;
        Debug.Log("2");
        VisitDialogue_stmt(intro);
        var menu_item = context.menu_item();
        foreach (var item in menu_item)
        {
            var block = item.block();
            var option = item.option.Text.Trim('"');
            Debug.Log($"选项: {option}");
            foreach (var line in block.statement())
            {
                Debug.Log("test");
            }
        }
        return null;
    }
}