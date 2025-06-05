/* using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Text;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.InputSystem.iOS;

namespace Assets.Scripts.DSP.Core.Deprecated
{
    public class Parser
    {

        public CommandSet TryParse(string scriptContent)
        {
            try
            {
                return Parse(scriptContent);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        private CommandSet Parse(string scriptContent)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
                throw new Exception("Script content is empty.");

            var lines = Preprocess(scriptContent, out var indents, out var lineNums);
            Stack<(Command, int)> parents = new();
            CommandSet commandGroup = new();

            for (int i = 0; i < lines.Count; i++)
            {
                var tokens = Tokenrize(lines[i]);
                if (tokens.Count == 0) continue;
                var indent = indents[i];
                var lineNum = lineNums[i];

                //先找所有带关键词的，不带关键词默认是对话
                // label 特殊处理，不装进函数，涉及列表较多
                // TODO

                if (tokens[0] == "label")
                {
                    if (tokens.Count != 3 || tokens[2] != ":" || indent != 0)
                        throw new Exception($"Parser Error: {lines[i]}\n[Line {lineNum}]");
                    var label = tokens[1];
                    if (commandGroup.LabelMap.ContainsKey(label))
                        throw new Exception($"Parser Error: Duplicate label: {label}\nLine {lineNum}");

                    var cmd = new LabelCommand
                    {
                        Label = label,
                        LineNum = lineNum
                    };

                   // commandGroup.Labels.Add(cmd);
                    // commandGroup.LabelMap.Add(label, commandGroup.Labels.Count - 1);

                    parents.Clear();
                    parents.Push((cmd, indent));
                    continue;
                }
                else if (parents.Count == 0 || indent < 1)
                {
                    throw new Exception($"Parser Error: Line must be inside a label.\n[Line {lineNum}]");
                }

                if (tokens[0] == "sync")
                {
                    var cmd = ParseDialogue(tokens, lineNum);
                    AddToParent(cmd, parents, indent);
                    continue;
                }

                if (tokens[0] == "jump")
                {
                    var cmd = ParseJump(tokens, lineNum);
                    AddToParent(cmd, parents, indent);
                    continue;
                }

                if (tokens[0] == "menu" && tokens[1] == ":")
                {
                    var cmd = ParseMenu(tokens, lineNum);
                    AddToParent(cmd, parents, indent);
                    continue;
                }
                if (tokens[0].StartsWith("\"") && tokens[0].EndsWith("\"") && tokens[1] == ":")
                {
                    while (parents.Peek().Item1.Type == CommandType.Menu)
                    {
                        if (parents.Count == 1)
                            throw new Exception($"Parser Error: No parent found for menu command.\n[Line {lineNum}]");
                        parents.Pop();
                    }
                }

            }

            throw new NotImplementedException();
        }

        private DialogueCommand ParseDialogue(List<string> tokens, int lineNum)
        {
            var cmd = new DialogueCommand
            {
                LineNum = lineNum
            };

            bool afterWith = false;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (afterWith)
                {
                    if (cmd.Text == null)
                        throw new Exception($"Parser Error: No text found before tags.\n[Line {lineNum}]");
                    cmd.Tags.Add(tokens[i]);
                }
                else if (tokens[i] == "sync")
                {
                    if (i != 0)
                        throw new Exception($"Parser Error: Sync must be the first token.\n[Line {lineNum}]");
                    if (cmd.IsSync)
                        throw new Exception($"Parser Error: Duplicate sync keyword found.\n[Line {lineNum}]");
                    cmd.IsSync = true;
                    tokens.RemoveAt(i);
                    i--;
                }
                else if (tokens[i] == "with")
                {
                    afterWith = true;
                }
                else if (tokens[i].StartsWith("\"") && tokens[i].EndsWith("\""))
                {
                    if (cmd.Text != null && cmd.Text != string.Empty)
                        throw new Exception($"Parser Error: Duplicate text found.\n[Line {lineNum}]");
                    cmd.Text = tokens[i].Trim('\"');
                }
                else
                {
                    if (cmd.Speaker != null && cmd.Speaker != string.Empty)
                        throw new Exception($"Parser Error: Duplicate speaker found.\n[Line {lineNum}]");
                    cmd.Speaker = tokens[i];
                }
            }

            GetInnerCmd(cmd);
            GetInnerExpr(cmd);

            if (cmd.Text == null || cmd.Text == string.Empty)
                throw new Exception($"Parser Error: No text found.\n[Line {lineNum}]");
            return cmd;
        }

        private JumpCommand ParseJump(List<string> tokens, int lineNum)
        {
            if (tokens.Count != 2 || tokens[1] != "jump")
                throw new Exception($"Parser Error: Invalid jump command.\n[Line {lineNum}]");

            var cmd = new JumpCommand
            {
                Target = tokens[1],
                LineNum = lineNum
            };
            return cmd;
        }

        private MenuCommand ParseMenu(List<string> tokens, int lineNum)
        {
            return new MenuCommand
            {
                Intro = null,
                LineNum = lineNum
            };
        }

        private void GetInnerCmd(DialogueCommand cmd)
        {
            var matches = Regex.Matches(cmd.Text, @"\[(.*?)\]");
            Regex.Replace(cmd.Text, @"\[(.*?)\]", "[@@]");
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var innerCmdStr = match.Groups[1].Value;
                    var innerCmd = new CallCommand
                    {
                        Args = new List<string>(innerCmdStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)),
                        LineNum = cmd.LineNum
                    };
                    cmd.InnerCommands.Add(innerCmd);
                }
            }
        }

        private void GetInnerExpr(DialogueCommand cmd)
        {
            var matches = Regex.Matches(cmd.Text, @"\{(.*?)\}");
            Regex.Replace(cmd.Text, @"\{(.*?)\}", "[$$]");
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var innerExprStr = match.Groups[1].Value;
                    cmd.InnerExpressions.Add(new ExpressionTree(new List<string> { innerExprStr }));
                }
            }
        }

        private void AddToParent(Command command, Stack<(Command, int)> parents, int indent)
        {
            //TODO
        }

        private List<string> Tokenrize(string line)
        {
            List<string> tokens = new();
            bool inQuotes = false;
            bool escapeNext = false;
            string currentToken = string.Empty;

            for (int i = 0; i < line.Length; i++)
            {
                char current = line[i];

                if (escapeNext)
                {
                    currentToken += current;
                    escapeNext = false;
                    continue;
                }

                if (current == '\\')
                {
                    escapeNext = true;
                    currentToken += current;
                    continue;
                }

                if (current == '\"')
                {
                    inQuotes = !inQuotes;
                    currentToken += current;
                    continue;
                }

                if (!inQuotes)
                {
                    if (current == ' ')
                    {
                        if (!string.IsNullOrWhiteSpace(currentToken))
                        {
                            tokens.Add(currentToken.Trim());
                            currentToken = string.Empty;
                        }
                    }
                    else if (current == ':')
                    {
                        if (!string.IsNullOrWhiteSpace(currentToken))
                        {
                            tokens.Add(currentToken.Trim());
                            currentToken = string.Empty;
                        }
                        tokens.Add(":");
                        continue;
                    }
                }
                currentToken += current;
            }

            if (!string.IsNullOrWhiteSpace(currentToken))
            {
                tokens.Add(currentToken.Trim());
            }

            return tokens;
        }

        private List<string> Preprocess(string content, out List<int> indents, out List<int> lineNums)
        {
            content = RemoveBlockComment(content);
            var rawLines = content.Split(new[] { '\r', '\n' });

            var lines = new List<string>();
            indents = new List<int>();
            lineNums = new List<int>();
            for (int i = 0; i < rawLines.Length; i++)
            {
                var line = rawLines[i];
                line = RemoveLineComment(line);
                if (string.IsNullOrWhiteSpace(line)) continue;
                lines.Add(line.Trim());
                indents.Add(IndentCounter(line));
                lineNums.Add(i + 1);
            }
            return lines;
        }

        private string RemoveBlockComment(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return content;
            return Regex.Replace(content, "\"{3}.*?\"{3}", string.Empty, RegexOptions.Singleline);
        }

        private string RemoveLineComment(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return line;

            var result = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char current = line[i];

                if (current == '\"')
                {
                    inQuotes = !inQuotes;
                    result.Append(current);
                    continue;
                }

                if (current == '#' && !inQuotes)
                {
                    // 找到注释开始位置，直接截断字符串
                    return result.ToString();
                }

                result.Append(current);
            }

            return result.ToString();
        }

        private int IndentCounter(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return 0;

            int indentCount = 0;
            int spaceCount = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                {
                    spaceCount++;
                    if (spaceCount == 4)
                    {
                        indentCount++;
                        spaceCount = 0;
                    }
                }
                else if (c == '\t')
                {
                    indentCount++;
                }
                else
                {
                    break;
                }
            }

            return indentCount;
        }
    }
} */