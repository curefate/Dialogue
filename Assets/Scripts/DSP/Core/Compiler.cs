using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Assets.Scripts.DSP.Core;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;

public class Compiler
{
    private readonly InstructionBuilder _instructionBuilder = new();

    public List<InstructionBlock> Compile(string input)
    {
        input = PreProcess(input);
        var inputStream = new AntlrInputStream(input);
        var lexer = new DSLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new DSParser(tokens);
        var tree = parser.program();
        var labels = new List<InstructionBlock>();
        foreach (var label_block in tree.label_block())
        {
            var new_label = new InstructionBlock
            {
                LabelName = label_block.label.Text
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
        ret.Concat("\n");
        return ret;
    }
}

public class InstructionBuilder : DSParserBaseVisitor<IIRInstruction>
{
    public List<InstructionBlock> LabelBlocks { get; } = new List<InstructionBlock>();
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
        var label_block = new InstructionBlock { LabelName = labelName };
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
            RawText = context.text.Text.Trim('"'),
        };
        // TODO
        inst.Text = inst.RawText;
        foreach (var tag in context._tags)
        {
            inst.Tags.Add(tag.Text[1..]);
        }
        return inst;
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
            IsSync = context.SYNC() != null,
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
        if (context._conditions.Count < context._blocks.Count)
        {
            // Handle the last else block
            var else_block = context._blocks[^1];
            foreach (var stmt in else_block.statement())
            {
                var instruction = Visit(stmt);
                if (instruction != null)
                {
                    current_inst.FalseBranch.Add(instruction);
                }
            }
        }
        return inst;
    }
}

public class ExpressionBuilder : DSParserBaseVisitor<DSExpression>
{
    public override DSExpression VisitExpression([NotNull] DSParser.ExpressionContext context)
    {
        if (context.expr_logical_and().Length > 1)
        {
            DSExpression result = Visit(context.expr_logical_and(0));
            for (int i = 1; i < context.expr_logical_and().Length; i++)
            {
                result = DSExpression.OrElse(result, Visit(context.expr_logical_and(i)));
            }
            return result;
        }
        return Visit(context.expr_logical_and(0));
    }

    public override DSExpression VisitExpr_logical_and([NotNull] DSParser.Expr_logical_andContext context)
    {
        if (context.expr_equality().Length > 1)
        {
            DSExpression result = Visit(context.expr_equality(0));
            for (int i = 1; i < context.expr_equality().Length; i++)
            {
                result = DSExpression.AndAlso(result, Visit(context.expr_equality(i)));
            }
            return result;
        }
        return Visit(context.expr_equality(0));
    }

    public override DSExpression VisitExpr_equality([NotNull] DSParser.Expr_equalityContext context)
    {
        if (context.expr_comparison().Length > 1)
        {
            DSExpression result = Visit(context.expr_comparison(0));
            for (int i = 1; i < context.expr_comparison().Length; i++)
            {
                var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between comparisons
                var nextExpr = Visit(context.expr_comparison(i));
                result = op switch
                {
                    "==" => DSExpression.Equal(result, nextExpr),
                    "!=" => DSExpression.NotEqual(result, nextExpr),
                    _ => throw new NotSupportedException($"Unsupported operator: {op}")
                };
            }
            return result;
        }
        return Visit(context.expr_comparison(0));
    }

    public override DSExpression VisitExpr_comparison([NotNull] DSParser.Expr_comparisonContext context)
    {
        if (context.expr_term().Length > 1)
        {
            DSExpression result = Visit(context.expr_term(0));
            for (int i = 1; i < context.expr_term().Length; i++)
            {
                var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between terms
                var nextExpr = Visit(context.expr_term(i));
                result = op switch
                {
                    "<" => DSExpression.LessThan(result, nextExpr),
                    ">" => DSExpression.GreaterThan(result, nextExpr),
                    "<=" => DSExpression.LessThanOrEqual(result, nextExpr),
                    ">=" => DSExpression.GreaterThanOrEqual(result, nextExpr),
                    _ => throw new NotSupportedException($"Unsupported operator: {op}")
                };
            }
            return result;
        }
        return Visit(context.expr_term(0));
    }

    public override DSExpression VisitExpr_term([NotNull] DSParser.Expr_termContext context)
    {
        if (context.expr_factor().Length > 1)
        {
            DSExpression result = Visit(context.expr_factor(0));
            for (int i = 1; i < context.expr_factor().Length; i++)
            {
                var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between factors
                var nextExpr = Visit(context.expr_factor(i));
                result = op switch
                {
                    "+" => DSExpression.Add(result, nextExpr),
                    "-" => DSExpression.Subtract(result, nextExpr),
                    _ => throw new NotSupportedException($"Unsupported operator: {op}")
                };
            }
            return result;
        }
        return Visit(context.expr_factor(0));
    }

    public override DSExpression VisitExpr_factor([NotNull] DSParser.Expr_factorContext context)
    {
        if (context.expr_unary().Length > 1)
        {
            DSExpression result = Visit(context.expr_unary(0));
            for (int i = 1; i < context.expr_unary().Length; i++)
            {
                var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between unary expressions
                var nextExpr = Visit(context.expr_unary(i));
                result = op switch
                {
                    "*" => DSExpression.Multiply(result, nextExpr),
                    "/" => DSExpression.Divide(result, nextExpr),
                    "%" => DSExpression.Modulo(result, nextExpr),
                    _ => throw new NotSupportedException($"Unsupported operator: {op}")
                };
            }
            return result;
        }
        return Visit(context.expr_unary(0));
    }

    public override DSExpression VisitExpr_unary([NotNull] DSParser.Expr_unaryContext context)
    {
        var op = context.PLUS() ?? context.MINUS() ?? context.EXCLAMATION();
        if (op != null)
        {
            var operand = Visit(context.expr_primary());
            return op.GetText() switch
            {
                "+" => operand, // Unary plus, no change
                "-" => DSExpression.Negate(operand), // Unary minus
                "!" => DSExpression.Not(operand), // Logical NOT
                _ => throw new NotSupportedException($"Unsupported unary operator: {op.GetText()}")
            };
        }
        return Visit(context.expr_primary());
    }

    public override DSExpression VisitExpr_primary([NotNull] DSParser.Expr_primaryContext context)
    {
        if (context.VARIABLE() != null)
        {
            var varName = context.VARIABLE().GetText();
            return DSExpression.Variable(varName[1..]); // Remove the '$' prefix
        }
        else if (context.NUMBER() != null)
        {
            var numText = context.NUMBER().GetText();
            if (numText.Contains('.'))
            {
                return DSExpression.Constant(float.Parse(numText));
            }
            return DSExpression.Constant(int.Parse(numText));
        }
        else if (context.BOOL() != null)
        {
            return DSExpression.Constant(bool.Parse(context.BOOL().GetText()));
        }
        else if (context.STRING() != null)
        {
            return DSExpression.Constant(context.STRING().GetText().Trim('"'));
        }
        else if (context.LPAR() != null)
        {
            return Visit(context.expression());
        }
        else
        {
            throw new NotSupportedException($"Unsupported expression: {context.GetText()}");
        }
    }
}