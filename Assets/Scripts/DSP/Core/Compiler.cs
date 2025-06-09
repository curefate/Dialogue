using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Antlr4.Runtime.Misc;

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
}

public class ExpressionBuilder : DSParserBaseVisitor<Expression>
{
    public override Expression VisitExpression([NotNull] DSParser.ExpressionContext context)
    {
        return base.VisitExpression(context); // TODO
    }
}