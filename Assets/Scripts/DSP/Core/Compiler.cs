using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class Compiler
{
    private readonly InstructionBuilder _instructionBuilder = new();
    public List<LabelBlock> Compile(string input)
    {
        throw new NotImplementedException("Compile method is not implemented yet.");
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
        throw new NotImplementedException("Dialogue_stmt is not implemented yet.");
    }// TODO

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
            var parsedArg = ParseArgument(arg);
            inst.Arguments.Add(parsedArg);
        }
        return inst;
    }

    public override IIRInstruction VisitSet_stmt([NotNull] DSParser.Set_stmtContext context)
    {
        var inst = new IR_Set
        {
            VariableName = context.VARIABLE().GetText(),
            Symbol = context.eq.Text,
            Value = ParseArgument(context.value),
        };
        return inst;
    }

    public override IIRInstruction VisitIf_stmt([NotNull] DSParser.If_stmtContext context)
    {
        var inst = new IR_If();
        for (int i = 0; i < context._conditions.Count; i++)
        {
            var condition = context._conditions[i];
            var block = context._blocks[i];
            // TODO
        }
        throw new NotImplementedException("If_stmt is not implemented yet.");
    }

    private KeyValuePair<object, VariableType> ParseArgument(DSParser.ArgumentContext arg)
    {
        if (arg.expression() != null)
        {
            var expr = _expressionBuilder.VisitExpression(arg.expression());
            return new KeyValuePair<object, VariableType>(expr, VariableType.Expression);
        }
        else if (arg.STRING() != null)
        {
            var str = arg.STRING().GetText().Trim('"'); // Remove quotes
            return new KeyValuePair<object, VariableType>(str, VariableType.String);
        }
        else if (arg.BOOL() != null)
        {
            var boolValue = arg.BOOL().Symbol.Type == DSLexer.TRUE;
            return new KeyValuePair<object, VariableType>(boolValue, VariableType.Boolean);
        }
        else if (arg.NUMBER() != null)
        {
            var numStr = arg.NUMBER().GetText();
            if (int.TryParse(numStr, out var intValue))
            {
                return new KeyValuePair<object, VariableType>(intValue, VariableType.Integer);
            }
            else if (float.TryParse(numStr, out var floatValue))
            {
                return new KeyValuePair<object, VariableType>(floatValue, VariableType.Float);
            }
            else
            {
                throw new ArgumentException($"Invalid number format: {numStr}");
            }
        }
        else if (arg.VARIABLE() != null)
        {
            var varName = arg.VARIABLE().GetText();
            // Assuming we have a way to resolve variable types, e.g., from a symbol table
            // For now, we assume it's a string variable
            return new KeyValuePair<object, VariableType>(varName, VariableType.Variable);
        }
        else
        {
            throw new ArgumentException("Invalid argument type.");
        }
    }
}

public class ExpressionBuilder : DSParserBaseVisitor<Expression>
{
    public override Expression VisitExpression([NotNull] DSParser.ExpressionContext context)
    {
        return base.VisitExpression(context); // TODO
    }
}