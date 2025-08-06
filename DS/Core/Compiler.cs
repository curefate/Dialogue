namespace DS.Core
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;

    public class Compiler
    {
        private readonly InstructionBuilder _instructionBuilder = new();

        public List<LabelBlock> CompileRawText(string input)
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new DSLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new DSParser(tokens);
            var tree = parser.program();
            var labelBlocks = new List<LabelBlock>();
            foreach (var lb in tree.label_block())
            {
                var newLabelBlock = new LabelBlock(lb.label.Text, parser.InputStream.SourceName);
                foreach (var stmt in lb.statement())
                {
                    var instruction = _instructionBuilder.Visit(stmt);
                    if (instruction != null)
                    {
                        newLabelBlock.Instructions.Add(instruction);
                    }
                }
                labelBlocks.Add(newLabelBlock);
            }
            return labelBlocks;
        }

        public List<LabelBlock> Compile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException($"File not found: {filePath}");
            }
            var fileStream = new AntlrFileStream(filePath);
            var lexer = new DSLexer(fileStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new DSParser(tokens);
            var tree = parser.program();
            var labelBlocks = new List<LabelBlock>();
            foreach (var lb in tree.label_block())
            {
                var newLabelBlock = new LabelBlock(lb.label.Text, parser.InputStream.SourceName);
                foreach (var stmt in lb.statement())
                {
                    var instruction = _instructionBuilder.Visit(stmt);
                    if (instruction != null)
                    {
                        newLabelBlock.Instructions.Add(instruction);
                    }
                }
                labelBlocks.Add(newLabelBlock);
            }
            return labelBlocks;
        }
    }

    internal class InstructionBuilder : DSParserBaseVisitor<IRInstruction>
    {
        private readonly ExpressionBuilder _expressionBuilder = new();

        public override IRInstruction VisitDialogue_stmt([NotNull] DSParser.Dialogue_stmtContext context)
        {
            var inst = new IR_Dialogue
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                SpeakerName = context.ID()?.GetText() ?? string.Empty,
                TextNode = _expressionBuilder.Visit(context.text).Root as FStringNode
    ?? throw new ArgumentException($"Dialogue text cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]"),
            };
            foreach (var tag in context._tags)
            {
                inst.Tags.Add(tag.Text[1..]);
            }
            return inst;
        }

        public override IRInstruction VisitMenu_stmt([NotNull] DSParser.Menu_stmtContext context)
        {
            var inst = new IR_Menu()
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
            };
            var options = context._options
    ?? throw new ArgumentException($"Menu options cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
            foreach (var option in options)
            {
                inst.OptionTextNodes.Add(_expressionBuilder.Visit(option.text).Root as FStringNode
    ?? throw new ArgumentException($"Menu option text cannot be null. [Ln: {option.Start.Line}, Fp: {option.Start.InputStream.SourceName}]"));
                var block = option.block()
    ?? throw new ArgumentException($"Menu option block cannot be null. [Ln: {option.Start.Line}, Fp: {option.Start.InputStream.SourceName}]");
                var actions = new List<IRInstruction>();
                foreach (var stmt in block.statement())
                {
                    var instruction = Visit(stmt);
                    if (instruction != null)
                    {
                        actions.Add(instruction);
                    }
                }
                inst.Blocks.Add(actions);
            }
            return inst;
        }

        public override IRInstruction VisitJump_stmt([NotNull] DSParser.Jump_stmtContext context)
        {
            var inst = new IR_Jump
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                TargetLabel = context.label.Text
            };
            return inst;
        }

        public override IRInstruction VisitTour_stmt([NotNull] DSParser.Tour_stmtContext context)
        {
            var inst = new IR_Tour
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                TargetLabel = context.label.Text
            };
            return inst;
        }

        public override IRInstruction VisitCall_stmt([NotNull] DSParser.Call_stmtContext context)
        {
            var inst = new IR_Call
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                FunctionName = context.func_name.Text
            };
            var args = context._args
    ?? throw new ArgumentException($"Call arguments cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
            foreach (var arg in args)
            {
                if (arg == null)
                {
                    throw new ArgumentException($"Call argument expression cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                }
                var expr = _expressionBuilder.Visit(arg)
    ?? throw new ArgumentException($"Call argument expression cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                inst.Arguments.Add(expr);
            }
            return inst;
        }

        public override IRInstruction VisitSet_stmt([NotNull] DSParser.Set_stmtContext context)
        {
            var inst = new IR_Set
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                VariableName = context.VARIABLE().GetText(),
                Symbol = context.eq.Text,
                Value = _expressionBuilder.Visit(context.value),
            };
            return inst;
        }

        public override IRInstruction VisitIf_stmt([NotNull] DSParser.If_stmtContext context)
        {
            var inst = new IR_If()
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                Condition = _expressionBuilder.Visit(context._conditions[0]),
            };
            var current_inst = inst;
            for (int i = 0; i < context._conditions.Count; i++)
            {
                var condition = context._conditions[i];
                var block = context._blocks[i];
                if (i != 0)
                {
                    var new_inst = new IR_If()
                    {
                        LineNum = condition.Start.Line,
                        FilePath = condition.Start.InputStream.SourceName,
                        Condition = _expressionBuilder.Visit(condition),
                    };
                    current_inst.FalseBranch.Add(new_inst);
                    current_inst = new_inst;
                }
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

    internal class ExpressionBuilder : DSParserBaseVisitor<DSExpression>
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
            else if (context.fstring() != null)
            {
                return Visit(context.fstring());
            }
            else if (context.LPAR() != null)
            {
                return Visit(context.expression());
            }
            else if (context.embedded_call() != null)
            {
                return Visit(context.embedded_call());
            }
            else
            {
                throw new NotSupportedException($"Unsupported expression: {context.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
            }
        }

        public override DSExpression VisitEmbedded_call([NotNull] DSParser.Embedded_callContext context)
        {
            if (context._args.Count > 0)
            {
                var args = new List<DSExpression>();
                foreach (var arg in context._args)
                {
                    args.Add(Visit(arg));
                }
                return DSExpression.Call(context.func_name.Text, args.ToArray());
            }
            else
            {
                return DSExpression.Call(context.func_name.Text);
            }
        }

        public override DSExpression VisitFstring([NotNull] DSParser.FstringContext context)
        {
            var fragments = new List<string>();
            var embed = new List<DSExpression>();
            foreach (var child in context.children)
            {
                if (child is DSParser.String_fragmentContext stringFragment)
                {
                    if (stringFragment.STRING_CONTEXT() != null)
                    {
                        fragments.Add(stringFragment.GetText());
                    }
                    else if (stringFragment.STRING_ESCAPE() != null)
                    {
                        switch (stringFragment.GetText())
                        {
                            case "\\b":
                                fragments.Add("\b");
                                break;
                            case "\\t":
                                fragments.Add("\t");
                                break;
                            case "\\n":
                                fragments.Add("\n");
                                break;
                            case "\\f":
                                fragments.Add("\f");
                                break;
                            case "\\r":
                                fragments.Add("\r");
                                break;
                            case "\\'":
                                fragments.Add("'");
                                break;
                            case "\\\"":
                                fragments.Add("\"");
                                break;
                            case "\\\\":
                                fragments.Add("\\");
                                break;
                            case "{{":
                                fragments.Add("{");
                                break;
                            case "}}":
                                fragments.Add("}");
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported string escape: {stringFragment.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                        }

                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported string fragment: {stringFragment.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                    }
                }
                else if (child is DSParser.Embedded_exprContext embeddedExpr)
                {
                    if (embeddedExpr.embedded_call() != null)
                    {
                        embed.Add(Visit(embeddedExpr.embedded_call()));
                    }
                    else if (embeddedExpr.expression() != null)
                    {
                        embed.Add(Visit(embeddedExpr.expression()));
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported embedded expression: {embeddedExpr.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                    }
                    fragments.Add(FStringNode.EmbedSign);
                }
            }
            return DSExpression.FString(fragments, embed);
        }
    }
}