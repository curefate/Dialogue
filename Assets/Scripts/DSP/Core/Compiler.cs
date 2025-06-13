using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class Compiler
{
    private readonly InstructionBuilder _instructionBuilder = new();

    public List<LabelBlock> Compile(string input)
    {
        input = PreProcess(input);
        var inputStream = new AntlrInputStream(input);
        var lexer = new DSLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DSParser(tokens);
        var tree = parser.program();
        var labels = new List<LabelBlock>();
        foreach (var label_block in tree.label_block())
        {
            var new_label = new LabelBlock
            {
                Label = label_block.label.Text
            };
            foreach (var stmt in label_block.statement())
            {
                var instruction = _instructionBuilder.Visit(stmt);
                if (instruction != null)
                {
                    new_label.Instructions.Add(instruction);
                }
            }
            labels.Add(new_label);
        }
        return labels;
    }

    private string PreProcess(string scriptContent)
    {
        string ret = scriptContent.Replace("\t", "    ");
        ret += "\n";
        return ret;
    }
}

public class InstructionBuilder : DSParserBaseVisitor<IIRInstruction>
{
    public List<LabelBlock> LabelBlocks { get; } = new List<LabelBlock>();
    private readonly ExpressionBuilder _expressionBuilder = new();

    public override IIRInstruction VisitProgram([NotNull] DSParser.ProgramContext context)
    {
        LabelBlocks.Clear();
        return base.VisitProgram(context);
    }

    public override IIRInstruction VisitLabel_block([NotNull] DSParser.Label_blockContext context)
    {
        var labelName = context.label.Text;
        var statements = context.statement();
        var label_block = new LabelBlock { Label = labelName };
        foreach (var stmt in statements)
        {
            var instruction = Visit(stmt);
            if (instruction != null)
            {
                label_block.Instructions.Add(instruction);
            }
        }
        LabelBlocks.Add(label_block);
        return null;
    }

    public override IIRInstruction VisitDialogue_stmt([NotNull] DSParser.Dialogue_stmtContext context)
    {
        var inst = new IR_Dialogue
        {
            IsSync = context.SYNC() != null,
            Speaker = context.ID()?.GetText() ?? null,
            Text = context.text.Text.Trim('"')
        };
        foreach (var tag in context._tags)
        {
            inst.Tags.Add(tag.Text[1..]);
        }
        return inst;
        // TODO inner call cmd
    }

    public override IIRInstruction VisitMenu_stmt([NotNull] DSParser.Menu_stmtContext context)
    {
        var inst = new IR_Menu();
        var options = context._options ?? throw new ArgumentException("Menu options cannot be null.");
        foreach (var option in options)
        {
            var text = option.text.Text;
            var block = option.block() ?? throw new ArgumentException("Menu option block cannot be null.");
            inst.MenuOptions.Add(text);
            var actions = new List<IIRInstruction>();
            foreach (var stmt in block.statement())
            {
                var instruction = Visit(stmt);
                if (instruction != null)
                {
                    actions.Add(instruction);
                }
            }
            inst.MenuActions.Add(actions);
        }
        return inst;
    }

    public override IIRInstruction VisitJump_stmt([NotNull] DSParser.Jump_stmtContext context)
    {
        var inst = new IR_Jump { TargetLabel = context.label.Text };
        return inst;
    }

    public override IIRInstruction VisitTour_stmt([NotNull] DSParser.Tour_stmtContext context)
    {
        var inst = new IR_Tour { TargetLabel = context.label.Text };
        return inst;
    }

    public override IIRInstruction VisitCall_stmt([NotNull] DSParser.Call_stmtContext context)
    {
        var inst = new IR_Call
        {
            FunctionName = context.func_name.Text
        };
        var args = context._args ?? throw new ArgumentException("Call arguments cannot be null.");
        foreach (var arg in args)
        {
            if (arg == null)
            {
                throw new ArgumentException("Call argument expression cannot be null.");
            }
            var expr = _expressionBuilder.Visit(arg) ?? throw new ArgumentException("Call argument expression cannot be null.");
            inst.Arguments.Add(expr);
        }
        return inst;
    }

    public override IIRInstruction VisitSet_stmt([NotNull] DSParser.Set_stmtContext context)
    {
        var inst = new IR_Set
        {
            VariableName = context.VARIABLE().GetText(),
            Symbol = context.eq.Text,
            Value = _expressionBuilder.Visit(context.value),
        };
        return inst;
    }

    public override IIRInstruction VisitIf_stmt([NotNull] DSParser.If_stmtContext context)
    {
        var inst = new IR_If();
        var current_inst = inst;
        for (int i = 0; i < context._conditions.Count; i++)
        {
            // For the last else block without condition
            if (i == context._conditions.Count - 1)
            {
                var else_block = context._blocks[i + 1];
                foreach (var stmt in else_block.statement())
                {
                    var instruction = Visit(stmt);
                    if (instruction != null)
                    {
                        current_inst.TrueBranch.Add(instruction);
                    }
                }
                break;
            }

            var condition = context._conditions[i];
            var block = context._blocks[i];
            if (i != 0)
            {
                var new_inst = new IR_If();
                current_inst.FalseBranch.Add(new_inst);
                current_inst = new_inst;
            }
            current_inst.Condition = _expressionBuilder.Visit(condition);
            foreach (var stmt in block.statement())
            {
                var instruction = Visit(stmt);
                if (instruction != null)
                {
                    current_inst.TrueBranch.Add(instruction);
                }
            }
        }
        throw new NotImplementedException("If_stmt is not implemented yet.");
    }
}

// TODO read
public class ExpressionBuilder : DSParserBaseVisitor<Expression>
{
    private readonly Func<string, object> _valueResolver;

    // 内部使用的变量访问器类
    private class VariableAccessor
    {
        private readonly string _name;
        private readonly Func<string, object> _resolver;

        public VariableAccessor(string name, Func<string, object> resolver)
        {
            _name = name;
            _resolver = resolver;
        }

        public object GetValue() => _resolver(_name);
    }

    /// <summary>
    /// 解析表达式文本并生成可执行的委托
    /// </summary>
    /* public Func<object> Build(string expressionText)
    {
        var input = new AntlrInputStream(expressionText);
        var lexer = new DSLexer(input);
        var parser = new DSParser(new CommonTokenStream(lexer));
        var expr = Visit(parser.expression());

        // 包装成object返回类型
        if (expr.Type != typeof(object))
        {
            expr = Expression.Convert(expr, typeof(object));
        }

        return Expression.Lambda<Func<object>>(expr).Compile();
    } */

    #region 表达式节点处理方法
    public override Expression VisitExpression(DSParser.ExpressionContext context)
    {
        if (context.expr_logical_and().Length > 1)
        {
            Expression result = Visit(context.expr_logical_and(0));
            for (int i = 1; i < context.expr_logical_and().Length; i++)
            {
                result = Expression.OrElse(result, Visit(context.expr_logical_and(i)));
            }
            return result;
        }
        return Visit(context.expr_logical_and(0));
    }

    public override Expression VisitExpr_logical_and(DSParser.Expr_logical_andContext context)
    {
        if (context.expr_equality().Length > 1)
        {
            Expression result = Visit(context.expr_equality(0));
            for (int i = 1; i < context.expr_equality().Length; i++)
            {
                result = Expression.AndAlso(result, Visit(context.expr_equality(i)));
            }
            return result;
        }
        return Visit(context.expr_equality(0));
    }

    public override Expression VisitExpr_equality(DSParser.Expr_equalityContext context)
    {
        if (context.expr_comparison().Length > 1)
        {
            var left = Visit(context.expr_comparison(0));
            var right = Visit(context.expr_comparison(1));
            var op = context.GetChild(1).GetText();

            return op switch
            {
                "==" => Expression.Equal(left, right),
                "!=" => Expression.NotEqual(left, right),
                _ => throw new NotSupportedException($"unsupport symbol: {op}")
            };
        }
        return Visit(context.expr_comparison(0));
    }

    public override Expression VisitExpr_primary(DSParser.Expr_primaryContext context)
    {
        if (context.VARIABLE() != null)
        {
            // 处理变量（延迟解析）
            string varName = context.VARIABLE().GetText()[1..];
            var accessor = new VariableAccessor(varName, _valueResolver);
            return Expression.Call(
                Expression.Constant(accessor),
                typeof(VariableAccessor).GetMethod("GetValue")
            );
        }
        else if (context.NUMBER() != null)
        {
            // 处理数字常量
            var numText = context.NUMBER().GetText();
            if (numText.Contains('.'))
            {
                // 处理浮点数
                return Expression.Constant(double.Parse(numText));
            }
            // 处理整数
            return Expression.Constant(int.Parse(numText));
        }
        else if (context.BOOL() != null)
        {
            // 处理布尔常量
            return Expression.Constant(bool.Parse(context.BOOL().GetText()));
        }
        else if (context.STRING() != null)
        {
            // 处理字符串常量
            return Expression.Constant(context.STRING().GetText().Trim('"'));
        }
        else if (context.LPAR() != null)
        {
            // 处理括号表达式
            return Visit(context.expression());
        }
        throw new NotSupportedException($"unsupport expr: {context.GetText()}");
    }
    #endregion
}