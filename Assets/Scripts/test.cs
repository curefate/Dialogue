using Antlr4.Runtime;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

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
        /* try
        {
            // 必选部分
            string text = context.STRING().GetText().Trim('"');
            Debug.Log($"内容: {text}");

            // 可选说话者
            var speakerNode = context.ID();
            if (speakerNode != null)
            {
                Debug.Log($"说话者: {speakerNode.GetText()}");
            }

            // 可选标签（WITH后的ID）
            if (context.WITH() != null)
            {
                foreach (var tagNode in context.TAG().Skip(speakerNode != null ? 1 : 0))
                {
                    Debug.Log($"标签: {tagNode.GetText()}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析对话时出错: {ex.Message}");
        }
        return null; */
        Debug.Log(context.Start.Line);
        return null;
    }
}