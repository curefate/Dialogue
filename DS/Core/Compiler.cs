namespace DS.Core
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;

    public class Compiler
    {
        private readonly StatementBuilder _statementBuilder = new();
        private readonly HashSet<string> _impotedFiles = [];

        private Dictionary<string, LabelBlock> _compile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException($"File not found: {filePath}");
            }

            filePath = Path.GetFullPath(filePath);
            _impotedFiles.Add(filePath);

            var fileStream = new AntlrFileStream(filePath);
            var lexer = new DSLexer(fileStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new DSParser(tokens);
            var tree = parser.program();
            
            var labelDict = new Dictionary<string, LabelBlock>();

            foreach (var import in tree.import_stmt())
            {
                var importPath = import.path.Text;
                if (string.IsNullOrEmpty(importPath))
                {
                    throw new ArgumentException($"Import path cannot be empty in file: {filePath}");
                }
                if (!Path.IsPathFullyQualified(importPath))
                {
                    importPath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, importPath);
                }
                if (!File.Exists(importPath))
                {
                    throw new ArgumentException($"Import file not found: {importPath}");
                }
                if (!string.Equals(Path.GetExtension(importPath), ".ds", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid import file type: {importPath}. Only '.ds' files are allowed.");
                }
                if (_impotedFiles.Contains(importPath))
                {
                    continue;
                }

                var importedLabels = _compile(importPath);
                foreach (var kvp in importedLabels)
                {
                    if (labelDict.ContainsKey(kvp.Key))
                    {
                        throw new ArgumentException($"Duplicate label found: {kvp.Key} in file {filePath}");
                    }
                    labelDict.Add(kvp.Key, kvp.Value);
                }
            }

            foreach (var lb in tree.label_block())
            {
                if (labelDict.ContainsKey(lb.label.Text))
                {
                    throw new ArgumentException($"Duplicate label found: {lb.label.Text} in file {filePath}");
                }

                var newLabelBlock = new LabelBlock(lb.label.Text, parser.InputStream.SourceName);
                _statementBuilder.CurrentLabel = lb.label.Text;

                foreach (var stmt in lb.statement())
                {
                    var instruction = _statementBuilder.Visit(stmt);
                    if (instruction != null)
                    {
                        newLabelBlock.Instructions.Add(instruction);
                    }
                }

                labelDict.Add(lb.label.Text, newLabelBlock);
            }

            return labelDict;
        }

        public Dictionary<string, LabelBlock> Compile(string filePath)
        {
            _impotedFiles.Clear();

            return _compile(filePath);
        }
    }

    internal class StatementBuilder : DSParserBaseVisitor<Statement>
    {
        private readonly ExpressionBuilder _expressionBuilder = new();
        public string CurrentLabel { get => _expressionBuilder.CurrentLabel; set => _expressionBuilder.CurrentLabel = value; }

        public override Statement VisitDialogue_stmt([NotNull] DSParser.Dialogue_stmtContext context)
        {
            var inst = new Stmt_Dialogue
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                SpeakerName = context.ID()?.GetText() ?? string.Empty,
                TextNode = _expressionBuilder.Visit(context.text).Root as FStringNode
    ?? throw new ArgumentException($"(Compilation Error) Dialogue text cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]"),
            };
            foreach (var tag in context._tags)
            {
                inst.Tags.Add(tag.Text[1..]);
            }
            return inst;
        }

        public override Statement VisitMenu_stmt([NotNull] DSParser.Menu_stmtContext context)
        {
            var inst = new Stmt_Menu()
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
            };
            var options = context._options
    ?? throw new ArgumentException($"(Compilation Error) Menu options cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
            foreach (var option in options)
            {
                inst.OptionTextNodes.Add(_expressionBuilder.Visit(option.text).Root as FStringNode
    ?? throw new ArgumentException($"(Compilation Error) Menu option text cannot be null. [Ln: {option.Start.Line}, Fp: {option.Start.InputStream.SourceName}]"));
                var block = option.block()
    ?? throw new ArgumentException($"(Compilation Error) Menu option block cannot be null. [Ln: {option.Start.Line}, Fp: {option.Start.InputStream.SourceName}]");
                var actions = new List<Statement>();
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

        public override Statement VisitJump_stmt([NotNull] DSParser.Jump_stmtContext context)
        {
            var inst = new Stmt_Jump
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                TargetLabel = context.label.Text
            };
            return inst;
        }

        public override Statement VisitTour_stmt([NotNull] DSParser.Tour_stmtContext context)
        {
            var inst = new Stmt_Tour
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                TargetLabel = context.label.Text
            };
            return inst;
        }

        public override Statement VisitCall_stmt([NotNull] DSParser.Call_stmtContext context)
        {
            var inst = new Stmt_Call
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                FunctionName = context.func_name.Text
            };
            var args = context._args
    ?? throw new ArgumentException($"(Compilation Error) Call arguments cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
            foreach (var arg in args)
            {
                if (arg == null)
                {
                    throw new ArgumentException($"(Compilation Error) Call argument expression cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                }
                var expr = _expressionBuilder.Visit(arg)
    ?? throw new ArgumentException($"(Compilation Error) Call argument expression cannot be null. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                inst.Arguments.Add(expr);
            }
            return inst;
        }

        public override Statement VisitAssign_stmt([NotNull] DSParser.Assign_stmtContext context)
        {
            var value = _expressionBuilder.Visit(context.expression());
            var varName = context.VARIABLE().GetText();
            if (!varName.Contains('.'))
            {
                varName = varName.Insert(1, CurrentLabel + ".");
            }
            return context.symbol.Text switch
            {
                "=" => new Stmt_Assign
                {
                    LineNum = context.Start.Line,
                    FilePath = context.Start.InputStream.SourceName,
                    Expression = Expression.Assign(Expression.Variable(varName[1..]), value)
                },
                "+=" => new Stmt_Assign
                {
                    LineNum = context.Start.Line,
                    FilePath = context.Start.InputStream.SourceName,
                    Expression = Expression.Assign(Expression.Variable(varName[1..]), Expression.Add(Expression.Variable(varName[1..]), value))
                },
                "-=" => new Stmt_Assign
                {
                    LineNum = context.Start.Line,
                    FilePath = context.Start.InputStream.SourceName,
                    Expression = Expression.Assign(Expression.Variable(varName[1..]), Expression.Subtract(Expression.Variable(varName[1..]), value))
                },
                "*=" => new Stmt_Assign
                {
                    LineNum = context.Start.Line,
                    FilePath = context.Start.InputStream.SourceName,
                    Expression = Expression.Assign(Expression.Variable(varName[1..]), Expression.Multiply(Expression.Variable(varName[1..]), value))
                },
                "/=" => new Stmt_Assign
                {
                    LineNum = context.Start.Line,
                    FilePath = context.Start.InputStream.SourceName,
                    Expression = Expression.Assign(Expression.Variable(varName[1..]), Expression.Divide(Expression.Variable(varName[1..]), value))
                },
                "%=" => new Stmt_Assign
                {
                    LineNum = context.Start.Line,
                    FilePath = context.Start.InputStream.SourceName,
                    Expression = Expression.Assign(Expression.Variable(varName[1..]), Expression.Modulo(Expression.Variable(varName[1..]), value))
                },
                _ => throw new NotSupportedException($"(Compilation Error) Unsupported assignment operator: {context.symbol.Text} [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]")
            };
        }

        public override Statement VisitIf_stmt([NotNull] DSParser.If_stmtContext context)
        {
            var inst = new Stmt_If()
            {
                LineNum = context.Start.Line,
                FilePath = context.Start.InputStream.SourceName,
                Condition = context._conditions[0].NOT() != null
                    ? Expression.Not(_expressionBuilder.Visit(context._conditions[0].expression()))
                    : _expressionBuilder.Visit(context._conditions[0].expression()),
            };
            var current_inst = inst;
            for (int i = 0; i < context._conditions.Count; i++)
            {
                var condition = context._conditions[i];
                var block = context._blocks[i];
                if (i != 0)
                {
                    var new_inst = new Stmt_If()
                    {
                        LineNum = condition.Start.Line,
                        FilePath = condition.Start.InputStream.SourceName,
                        Condition = condition.NOT() != null
                            ? Expression.Not(_expressionBuilder.Visit(condition.expression()))
                            : _expressionBuilder.Visit(condition.expression()),
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

    internal class ExpressionBuilder : DSParserBaseVisitor<Expression>
    {
        public string CurrentLabel = string.Empty;

        public override Expression VisitExpression([NotNull] DSParser.ExpressionContext context)
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

        public override Expression VisitExpr_logical_and([NotNull] DSParser.Expr_logical_andContext context)
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

        public override Expression VisitExpr_equality([NotNull] DSParser.Expr_equalityContext context)
        {
            if (context.expr_comparison().Length > 1)
            {
                Expression result = Visit(context.expr_comparison(0));
                for (int i = 1; i < context.expr_comparison().Length; i++)
                {
                    var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between comparisons
                    var nextExpr = Visit(context.expr_comparison(i));
                    result = op switch
                    {
                        "==" => Expression.Equal(result, nextExpr),
                        "!=" => Expression.NotEqual(result, nextExpr),
                        _ => throw new NotSupportedException($"(Compilation Error) Unsupported operator: {op}")
                    };
                }
                return result;
            }
            return Visit(context.expr_comparison(0));
        }

        public override Expression VisitExpr_comparison([NotNull] DSParser.Expr_comparisonContext context)
        {
            if (context.expr_term().Length > 1)
            {
                Expression result = Visit(context.expr_term(0));
                for (int i = 1; i < context.expr_term().Length; i++)
                {
                    var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between terms
                    var nextExpr = Visit(context.expr_term(i));
                    result = op switch
                    {
                        "<" => Expression.LessThan(result, nextExpr),
                        ">" => Expression.GreaterThan(result, nextExpr),
                        "<=" => Expression.LessThanOrEqual(result, nextExpr),
                        ">=" => Expression.GreaterThanOrEqual(result, nextExpr),
                        _ => throw new NotSupportedException($"(Compilation Error) Unsupported operator: {op}")
                    };
                }
                return result;
            }
            return Visit(context.expr_term(0));
        }

        public override Expression VisitExpr_term([NotNull] DSParser.Expr_termContext context)
        {
            if (context.expr_factor().Length > 1)
            {
                Expression result = Visit(context.expr_factor(0));
                for (int i = 1; i < context.expr_factor().Length; i++)
                {
                    var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between factors
                    var nextExpr = Visit(context.expr_factor(i));
                    result = op switch
                    {
                        "+" => Expression.Add(result, nextExpr),
                        "-" => Expression.Subtract(result, nextExpr),
                        _ => throw new NotSupportedException($"(Compilation Error) Unsupported operator: {op}")
                    };
                }
                return result;
            }
            return Visit(context.expr_factor(0));
        }

        public override Expression VisitExpr_factor([NotNull] DSParser.Expr_factorContext context)
        {
            if (context.expr_unary().Length > 1)
            {
                Expression result = Visit(context.expr_unary(0));
                for (int i = 1; i < context.expr_unary().Length; i++)
                {
                    var op = context.GetChild(i * 2 - 1).GetText(); // Get the operator between unary expressions
                    var nextExpr = Visit(context.expr_unary(i));
                    result = op switch
                    {
                        "*" => Expression.Multiply(result, nextExpr),
                        "/" => Expression.Divide(result, nextExpr),
                        "%" => Expression.Modulo(result, nextExpr),
                        _ => throw new NotSupportedException($"(Compilation Error) Unsupported operator: {op}")
                    };
                }
                return result;
            }
            return Visit(context.expr_unary(0));
        }

        public override Expression VisitExpr_unary([NotNull] DSParser.Expr_unaryContext context)
        {
            var op = context.PLUS() ?? context.MINUS() ?? context.EXCLAMATION();
            if (op != null)
            {
                var operand = Visit(context.expr_primary());
                return op.GetText() switch
                {
                    "+" => operand, // Unary plus, no change
                    "-" => Expression.Negate(operand), // Unary minus
                    "!" => Expression.Not(operand), // Logical NOT
                    _ => throw new NotSupportedException($"(Compilation Error) Unsupported unary operator: {op.GetText()}")
                };
            }
            return Visit(context.expr_primary());
        }

        public override Expression VisitExpr_primary([NotNull] DSParser.Expr_primaryContext context)
        {
            if (context.VARIABLE() != null)
            {
                var varName = context.VARIABLE().GetText();
                if (!varName.Contains('.'))
                {
                    varName = varName.Insert(1, CurrentLabel + ".");
                }
                return Expression.Variable(varName[1..]); // Remove the '$' prefix
            }
            else if (context.NUMBER() != null)
            {
                var numText = context.NUMBER().GetText();
                if (numText.Contains('.'))
                {
                    return Expression.Constant(float.Parse(numText));
                }
                return Expression.Constant(int.Parse(numText));
            }
            else if (context.BOOL() != null)
            {
                return Expression.Constant(bool.Parse(context.BOOL().GetText()));
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
                throw new NotSupportedException($"(Compilation Error) Unsupported expression: {context.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
            }
        }

        public override Expression VisitEmbedded_call([NotNull] DSParser.Embedded_callContext context)
        {
            if (context._args.Count > 0)
            {
                var args = new List<Expression>();
                foreach (var arg in context._args)
                {
                    args.Add(Visit(arg));
                }
                return Expression.Call(context.func_name.Text, args.ToArray());
            }
            else
            {
                return Expression.Call(context.func_name.Text);
            }
        }

        public override Expression VisitFstring([NotNull] DSParser.FstringContext context)
        {
            var fragments = new List<string>();
            var embed = new List<Expression>();
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
                                throw new NotSupportedException($"(Compilation Error) Unsupported string escape: {stringFragment.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                        }

                    }
                    else
                    {
                        throw new NotSupportedException($"(Compilation Error) Unsupported string fragment: {stringFragment.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
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
                        throw new NotSupportedException($"(Compilation Error) Unsupported embedded expression: {embeddedExpr.GetText()}. [Ln: {context.Start.Line}, Fp: {context.Start.InputStream.SourceName}]");
                    }
                    fragments.Add(FStringNode.EmbedSign);
                }
            }
            return Expression.FString(fragments, embed);
        }
    }
}