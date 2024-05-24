using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal class MethodVisitor
{
    private static readonly Regex opPattern = new("\\s*(?'Operation'\\w+)[((](?'Operand'\\d{0,3}|0x[A-Fa-f0-9__]{1,4}|0b[0-1__]*[0-1])[))];", RegexOptions.Compiled);
    private static readonly Regex opXPattern = new("\\s*((?'JumpType'Call|Jump|Macro)<(?'Script'\\w+)>.+\\.)?(?'Operation'\\w+)\\s*[((](?'Operands'.*)[))];", RegexOptions.Compiled);
    private static readonly Regex opReturnPattern = new("\\s*var ((?'ReturnValue'\\w+)|([((](?'ReturnValues'.+)[))]))\\s*=\\s*(?'Operation'\\w+)[((](?'Operands'.*)[))];", RegexOptions.Compiled);

    private static readonly Regex commentPattern = new("//(?'Comment'.+)", RegexOptions.Compiled);
    private static readonly Regex labelPattern = new("(?'Label'.+):", RegexOptions.Compiled);
    private static readonly Regex gotoPattern = new("^\\s*goto (?'Label'.+);", RegexOptions.Compiled);
    private static readonly Regex branchPattern = new("if \\((?'Operation'.+)\\(\\)\\) goto (?'Label'.+);", RegexOptions.Compiled);

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    internal static void Visit(MethodDeclarationSyntax method, MethodVisitorContext context)
    {
        var writer = context.Writer;
        var parameters = new List<(byte index, string identifier, string type)>();
        byte paramIndex = 0;
        foreach (var parameter in method.ParameterList.Parameters)
        {
            parameters.Add((paramIndex++, parameter.Identifier.ToString(), parameter.Type.ToString()));

            if (!new[] { "byte", "ushort", "bool" }.Any(t => t == parameter.Type.ToString()))
            {
                context.ReportDiagnostic(Diagnostics.UnsupportedParameterType, parameter.GetLocation(), parameter.Type.ToString());
            }
        }

        int endlessloopIndex = 1;
        string? expectedAndOfLoop = null;
        var isMacro = method.AttributeLists.Any(att => att.Attributes.Any(a => a.Name.ToString() == "Macro"));

        var lines = method.Body.ToString().Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            var statement = GetLineStatement(method.Body.Statements, line);
            var location = statement != null ? statement.GetLocation() : Location.None;

            if (string.IsNullOrWhiteSpace(line))
            {
                writer.WriteEmptyLine();
                continue;
            }

            if (TryHandleIf(statement, line, context)) continue;
            if (TryHandleInvocation(statement, line, context)) continue;

            var match = commentPattern.Match(line);
            if (match.Success)
            {
                writer.WriteComment(match.Groups["Comment"].Value.TrimStart());
                continue;
            }

            match = labelPattern.Match(line);
            if (match.Success)
            {
                writer.WriteLabel(match.Groups["Label"].Value.Trim());
                continue;
            }

            match = gotoPattern.Match(line);
            if (match.Success)
            {
                writer.WriteJMPToLabelOpCode(match.Groups["Label"].Value.Trim());
                continue;
            }

            match = opReturnPattern.Match(line);
            if (match.Success)
            {
                parameters.Clear();
                paramIndex = 0;
                foreach (var parameter in match.Groups["ReturnValues"].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
                {
                    parameters.Add((paramIndex++, parameter, "byte"));
                }

                if (match.Groups["ReturnValue"].Success)
                {
                    parameters.Add((paramIndex++, match.Groups["ReturnValue"].Value, "byte"));
                }

                ParseOp(match.Groups["Operation"].Value, match.Groups["Operand"].Value, string.Empty, location, context);

                continue;
            }

            match = opPattern.Match(line);
            if (match.Success)
            {
                ParseOp(match.Groups["Operation"].Value, match.Groups["Operand"].Value, string.Empty, location, context);

                continue;
            }

            match = opXPattern.Match(line);
            if (match.Success)
            {
                var jumpType = match.Groups["JumpType"].Value;
                var script = match.Groups["Script"].Value;
                var operation = match.Groups["Operation"].Value;
                var operands = match.Groups["Operands"].Value.Split(',').Select(o => o.Trim());

                if (operands.Count() == 1)
                {
                    var parameter = parameters.SingleOrDefault(p => p.identifier == operands.Single());
                    if (parameter.identifier != null)
                    {
                        var opCode = operation switch
                        {
                            "LDAa" => "lda",
                            "LDXa" => "ldx",
                            "LDYa" => "ldy",
                            _ => null
                        };

                        if (opCode == null)
                        {
                            context.ReportDiagnostic(Diagnostics.InvalidOpCodeUsingParameters, location, operation);
                            continue;
                        }

                        switch (parameter.type)
                        {
                            case "bool": writer.WriteOpCode(opCode, parameter.index); break;
                            case "byte": writer.WriteOpCode(opCode, parameter.index); break;
                            default:
                                context.ReportDiagnostic(Diagnostics.UnsupportedUsingParameterType, location, operation);
                                continue;
                        }

                        continue;
                    }

                    // Hack to be removed after proper support of other script call with parameters
                    if (operands.Single() != ")")
                    {
                        ParseOp(operation, operands.Single(), string.Empty, location, context);

                        continue;
                    }
                }

                if (operands.Count() == 2 && (operands.ElementAt(1) == "X" || operands.ElementAt(1) == "Y"))
                {
                    var indexorRegister = operands.ElementAt(1) switch
                    {
                        "X" => "x",
                        "Y" => "y",
                        _ => null
                    };

                    if (indexorRegister == null)
                    {
                        context.ReportDiagnostic(Diagnostics.AbsoluteIndexedRegisterNotSupported, location, indexorRegister);
                        continue;
                    }

                    switch (operation)
                    {
                        case "LDA": writer.WriteOpCode("lda", Utilities.GetLabelName(operands.ElementAt(0)), indexorRegister); break;
                        case "STA": ParseOp(operation, operands.ElementAt(0), indexorRegister, location, context); break; //writer.WriteOpCode("sta", Utilities.GetLabelName(operands.ElementAt(0)), indexorRegister); break;
                        default:
                            context.ReportDiagnostic(Diagnostics.AbsoluteIndexedOpCodeNotSupported, location, operation);
                            continue;
                    }

                    continue;
                }

                if (!string.IsNullOrEmpty(script))
                {
                    var callingScope = script != (method.Parent as ClassDeclarationSyntax)?.Identifier.Text ? script : null;

                    // TODO Should Report Error if script not already referenced on the class using FileInclude
                    if (callingScope != null)
                        context.AddScriptReference($"{callingScope}.s");

                    if (jumpType == "Call")
                        writer.WriteJSROpCode(callingScope, Utilities.GetProcName(operation));
                    else if (jumpType == "Jump")
                        writer.WriteJMPOpCode(callingScope, Utilities.GetProcName(operation));
                    else if (jumpType == "Macro")
                        writer.WriteCallMacro(callingScope, operation, operands.ToArray());
                    else
                        context.ReportDiagnostic(Diagnostics.InvalidJumpTypeNotSupported, location, jumpType);

                    continue;
                }

                // TODO Use StoreData method instead
                byte index = 0;
                foreach (var operand in operands)
                {
                    if (bool.TryParse(operand, out var boolOperand))
                    {
                        writer.WriteOpCodeImmediate("lda", (byte)(boolOperand ? 1 : 0));
                        writer.WriteOpCode("sta", index++);
                    }
                    else if (byte.TryParse(operand, out var byteOperand))
                    {
                        writer.WriteOpCodeImmediate("lda", byteOperand);
                        writer.WriteOpCode("sta", index++);
                    }
                    else if (ushort.TryParse(operand, out var ushortOperand))
                    {
                        writer.WriteOpCodeImmediate("lda", (byte)(ushortOperand % 256));
                        writer.WriteOpCode("sta", index++);
                        writer.WriteOpCodeImmediate("lda", (byte)(ushortOperand / 256));
                        writer.WriteOpCode("sta", index++);
                    }
                    else
                    {
                        try
                        {
                            var value = Convert.ToInt32(operand, 16);
                            writer.WriteOpCodeImmediate("lda", (byte)(value % 256));
                            writer.WriteOpCode("sta", index++);
                            writer.WriteOpCodeImmediate("lda", (byte)(value / 256));
                            writer.WriteOpCode("sta", index++);
                        }
                        catch (FormatException)
                        {
                            context.ReportDiagnostic(Diagnostics.UnsupportedParameterType2, location, operand);
                            continue;
                        }
                    }
                }

                if (operation != "return") writer.WriteJSROpCode(null, Utilities.GetProcName(operation));
                continue;
            }

            match = branchPattern.Match(line);
            if (match.Success)
            {
                var branchingOpCode = match.Groups["Operation"].Value;
                var label = match.Groups["Label"].Value;

                switch (branchingOpCode)
                {
                    case "BEQ": writer.WriteBranchOpCode("beq", label); continue;
                    case "BNE": writer.WriteBranchOpCode("bne", label); continue;
                    case "BCC": writer.WriteBranchOpCode("bcc", label); continue;
                    case "BPL": writer.WriteBranchOpCode("bpl", label); continue;
                }

                context.ReportDiagnostic(Diagnostics.BranchingOpCodeNotSupported, location, branchingOpCode);
                continue;
            }

            if (line.Trim().StartsWith("return"))
            {
                var operand = line.Trim().Substring(7).TrimEnd(';').Trim();
                StoreData(new[] { operand }, location, context);
                continue;
            }

            // endless loop
            if (line.Trim() == "while (true)")
            {
                writer.WriteLabel($"endless_loop");
                expectedAndOfLoop = line.Replace("while (true)", "}");
                continue;
            }

            if (line == expectedAndOfLoop)
            {
                writer.WriteJMPToLabelOpCode("endless_loop");
                //endlessloopIndex++;
                expectedAndOfLoop = null;
                continue;
            }

            if (TryHandleFor(line, location, isMacro, context)) continue;
            if (TryHandleEndFor(line, location, context)) continue;
            
            if (TryHandleIfExit(line, location, context)) continue;

            // Ignore
            if (line.Trim() == "{" || line.Trim() == "}") continue;

            context.ReportDiagnostic(Diagnostics.InstructionNotSupported, location, line.Trim());
        }

        context.EnsureAllForLoopEnded(method.GetLocation());
        context.EnsureAllIfExitReached(method.GetLocation());

        foreach (var statement in method.Body.Statements)
        {
            if (statement is ExpressionStatementSyntax expression)
            {
            }
        }
    }

    private static StatementSyntax? GetLineStatement(SyntaxList<StatementSyntax> statements, string line)
    {
        var statement = statements.FirstOrDefault(s => s.ToString().Trim().Contains(line.Trim()));
        if (statement is WhileStatementSyntax whileStatement)
        {
            if (whileStatement.Statement is BlockSyntax block)
            {
                statement = block.Statements.FirstOrDefault(s => s.ToString().Trim().Contains(line.Trim()));
            }
        }
        else if (statement is LabeledStatementSyntax labeledStatement)
        {
            if (!line.Contains(labeledStatement.Identifier.ValueText))
            {
                if (labeledStatement.Statement is ExpressionStatementSyntax)
                {
                    statement = labeledStatement.Statement;
                }
            }
        }

        return statement;
    }

    private static readonly Regex forPattern = new("\\s*for \\(((?'VarType'\\w+) )?(?'Register'\\w+)? = (?'Init'\\d+); (?'Register2'\\w+) (?'Condition'[<|>|<=|>=]) (?'ConditionValue'\\d+); (?'Register3'\\w+)(?'Increment'.+)\\)", RegexOptions.Compiled);

    private static bool TryHandleFor(string line, Location location, bool isMacro, MethodVisitorContext context)
    {
        var match = forPattern.Match(line);
        if (match.Success)
        {
            // local variable used
            if (match.Groups["VarType"].Success)
            {
                context.ReportDiagnostic(Diagnostics.LocalVariableForLoopNotSupported, location, match.Groups["VarType"].Value);
                return true;
            }

            var register = match.Groups["Register"].Value;

            if (register != match.Groups["Register2"].Value || register != match.Groups["Register3"].Value)
            {
                context.ReportDiagnostic(Diagnostics.MultipleLoopVariableFound, location, line.Trim());
                return true;
            }

            if (register != "X" && register != "Y")
            {
                context.ReportDiagnostic(Diagnostics.InvalidLoopVariableFound, location, register);
                return true;
            }

            var init = match.Groups["Init"].Value;
            var condition = match.Groups["Condition"].Value;
            var conditionValue = match.Groups["ConditionValue"].Value;
            var increment = match.Groups["Increment"].Value;
            string labelName = "";

            context.Writer.WriteEmptyLine();

            // Init
            ParseOp(register == "X" ? "LDXi" : "LDYi", init, string.Empty, location, context);

            // Label
            if (isMacro)
            {
                labelName = "-";
                context.Writer.WriteUnnamedLabel();
            }
            else
            {
                labelName = $"loop{context.ForLoopIndex}_on_{register}";
                context.Writer.WriteLabel(labelName);
            }

            // Code

            // Increment
            var incrementOpCode = register == "X"
                ? (increment == "++" ? "INX" : "DEX")
                : (increment == "++" ? "INY" : "DEY");

            // Condition Evaluation
            var conditionOpCode = register == "X" ? $"CPXi" : "CPYi";
            var conditionOperand = conditionValue;

            // Branch
            var endloopPattern = line.Substring(0, line.IndexOf("for")) + "}";

            context.PushForLoopData(labelName, conditionOpCode, conditionOperand, incrementOpCode, endloopPattern);

            return true;
        }

        return false;
    }

    private static bool TryHandleEndFor(string line, Location location, MethodVisitorContext context)
    {
        if (context.IsLineMatchingEndLoop(line))
        {
            context.Writer.WriteEmptyLine();

            var forLoopData = context.PopForLoopData();

            // Increment
            ParseOp(forLoopData.IncrementOpCode, string.Empty, string.Empty, location, context);

            // Condition
            ParseOp(forLoopData.ConditionOpCode, forLoopData.ConditionOperand, string.Empty, location, context);

            // Branch
            string label = forLoopData.LabelName; 
            if (forLoopData.LabelName.Contains("-") && context.ForLoopDepth == 0)
            {
                label = string.Join("", Enumerable.Repeat("-", context.NestedLoopDepth + 1));
            }

            context.Writer.WriteBranchOpCode("bne", label);

            return true;
        }

        return false;
    }

    private static bool TryHandleIf(StatementSyntax? statementSyntax, string line, MethodVisitorContext context)
    {
        if (statementSyntax is IfStatementSyntax ifStatement && line.Contains("if ("))
        {
            if (ifStatement.Condition is BinaryExpressionSyntax binaryExpression)
            {
                var labelName = $"if{context.IdExitIndex}_exit";
                context.Writer.WriteComment($"if{context.IdExitIndex}_start");

                // (A & 0x01) != 0 pattern
                if (binaryExpression.Left is ParenthesizedExpressionSyntax parenthesizedExpression)
                {
                    if (binaryExpression.OperatorToken.ToString() != "!=" && binaryExpression.Right.ToString() != "0")
                    {
                        // Only != 0 supported
                        context.ReportDiagnostic(Diagnostics.InvalidIfLeftOperand, binaryExpression.OperatorToken.GetLocation());
                        return true;
                    }

                    if (parenthesizedExpression.Expression is BinaryExpressionSyntax bitwiseAndExpression)
                    {
                        if (bitwiseAndExpression.OperatorToken.ToString() != "&")
                        {
                            // Only & is supported
                            context.ReportDiagnostic(Diagnostics.InvalidIfLeftOperand, bitwiseAndExpression.OperatorToken.GetLocation());
                            return true;
                        }

                        var operation = bitwiseAndExpression.Left.ToString() switch
                        {
                            "A" => "ANDi",
                            _ => "",
                        };

                        if (string.IsNullOrEmpty(operation))
                        {
                            // Only A is supported for binary expression
                            context.ReportDiagnostic(Diagnostics.InvalidIfLeftOperand, bitwiseAndExpression.Left.GetLocation());
                            return true;
                        }

                        ParseOp(operation, bitwiseAndExpression.Right.ToString(), string.Empty, bitwiseAndExpression.Right.GetLocation(), context);
                        context.Writer.WriteBranchOpCode("beq", labelName);
                    }
                    else
                    {
                        // Only (A & 0x01) pattern supported
                        context.ReportDiagnostic(Diagnostics.InvalidIfLeftOperand, parenthesizedExpression.Expression.GetLocation());
                        return true;
                    }
                }
                else // A > 0 pattern
                {
                    // Compare
                    var operation = binaryExpression.Left.ToString() switch
                    {
                        "X" => "CPXi",
                        "Y" => "CPYi",
                        "A" => "CMP",
                        _ => "",
                    };

                    if (string.IsNullOrEmpty(operation))
                    {
                        context.ReportDiagnostic(Diagnostics.InvalidIfLeftOperand, binaryExpression.Left.GetLocation());
                        return true;
                    }

                    ParseOp(operation, binaryExpression.Right.ToString(), string.Empty, binaryExpression.Right.GetLocation(), context);

                    // Jump if condition is false
                    var branch = binaryExpression.OperatorToken.ToString() switch
                    {
                        "==" => "bne",
                        "!=" => "beq",
                        ">" => "bpl",
                        "<" => "bmi",
                        _ => "",
                    };

                    if (string.IsNullOrEmpty(branch))
                    {
                        context.ReportDiagnostic(Diagnostics.InvalidIfOperator, binaryExpression.OperatorToken.GetLocation());
                        return true;
                    }

                    context.Writer.WriteBranchOpCode(branch, labelName);
                }

                // ... body instructions
                context.Writer.WriteEmptyLine();

                var ifExitPattern = line.Substring(0, line.IndexOf("if")) + "}";
                context.PushIfExitData(labelName, ifExitPattern);

                return true;
            }
        }

        return false;
    }

    private static bool TryHandleIfExit(string line, Location location, MethodVisitorContext context)
    {
        if (context.IsLineMatchingIfExit(line))
        {
            context.Writer.WriteEmptyLine();

            var ifExitData = context.PopIfExitData();

            // Label for end if
            context.Writer.WriteLabel(ifExitData.LabelName);

            return true;
        }

        return false;
    }

    private static bool TryHandleInvocation(StatementSyntax? statementSyntax, string line, MethodVisitorContext context)
    {
        if (statementSyntax is ExpressionStatementSyntax expressionStatement)
        {
            if (expressionStatement.Expression is InvocationExpressionSyntax invocationExpression)
            {
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    if (memberAccessExpression.Expression is IdentifierNameSyntax identifierName)
                    {
                        var syntaxNode = statementSyntax.Parent;
                        while (syntaxNode is not ClassDeclarationSyntax)
                        {
                            syntaxNode = syntaxNode!.Parent;
                        }
                        var currentClass = (syntaxNode as ClassDeclarationSyntax)!.Identifier.ValueText;

                        context.TypeCache.Scan(statementSyntax.SyntaxTree, identifierName);

                        var className = identifierName.Identifier.ValueText;
                        var methodName = memberAccessExpression.Name.Identifier.ValueText;

                        var methodInfo = context.TypeCache.GetMethod(className, methodName);

                        // We don't want to use scope if we are in the same class
                        if (className == currentClass) className = null;

                        if (methodInfo != null)
                        {
                            if (methodInfo.IsProc || methodInfo.IsNoReturnProc)
                            {
                                StoreData(ConvertArguments(invocationExpression.ArgumentList.Arguments.ToArray(), context), statementSyntax.GetLocation(), context);
                            }

                            if (methodInfo.IsProc)
                            {
                                context.Writer.WriteJSROpCode(className, Utilities.GetProcName(methodInfo.Name));
                                return true;
                            }
                            else if (methodInfo.IsNoReturnProc)
                            {
                                context.Writer.WriteJMPOpCode(className, Utilities.GetProcName(methodInfo.Name));
                                return true;
                            }
                            else if (methodInfo.IsMacro)
                            {
                                var arguments = ConvertArguments(invocationExpression.ArgumentList.Arguments.ToArray(), context);

                                if (arguments.All(a => a != null))
                                {
                                    context.Writer.WriteCallMacro(null, Utilities.GetMacroName(methodInfo.Name), arguments);
                                    return true;
                                }
                            }

                            return true;
                        }
                    }
                }

                if (invocationExpression.Expression is IdentifierNameSyntax identifierName2)
                {
                    var syntaxNode = statementSyntax.Parent;
                    while (syntaxNode is not ClassDeclarationSyntax)
                    {
                        syntaxNode = syntaxNode!.Parent;
                    }
                    var currentClass = (syntaxNode as ClassDeclarationSyntax)!.Identifier.ValueText;

                    var model = context.Compilation.GetSemanticModel(identifierName2.SyntaxTree);

                    var memberSymbol = model.GetMemberGroup(identifierName2);
                    if (memberSymbol.Length == 0) return false;

                    var className = memberSymbol[0].ContainingType.Name;

                    var methodName = identifierName2.Identifier.ValueText;

                    var methodInfo = context.TypeCache.GetMethod(className, methodName);

                    if (methodInfo != null)
                    {
                        if (methodInfo.IsProc || methodInfo.IsNoReturnProc)
                        {
                            StoreData(ConvertArguments(invocationExpression.ArgumentList.Arguments.ToArray(), context), statementSyntax.GetLocation(), context);
                        }

                        if (methodInfo.IsProc)
                        {
                            context.Writer.WriteJSROpCode(null, Utilities.GetProcName(methodInfo.Name));
                            return true;
                        }
                        else if (methodInfo.IsNoReturnProc)
                        {
                            context.Writer.WriteJMPOpCode(null, Utilities.GetProcName(methodInfo.Name));
                            return true;
                        }
                        else if (methodInfo.IsMacro)
                        {
                            var arguments = ConvertArguments(invocationExpression.ArgumentList.Arguments.ToArray(), context);

                            if (arguments.All(a => a != null))
                            {
                                context.Writer.WriteCallMacro(null, Utilities.GetMacroName(methodInfo.Name), arguments);
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private static string[] ConvertArguments(ArgumentSyntax[] arguments, MethodVisitorContext context)
    {
        var result = new string[arguments.Length];

        for (int i = 0; i < arguments.Length; i++)
        {
            try
            {
                var expression = arguments[i].Expression;
                if (arguments[i].Expression is CastExpressionSyntax castExpression)
                {
                    if (castExpression.Expression is ParenthesizedExpressionSyntax parenthesizedExpression)
                    {
                        expression = parenthesizedExpression.Expression;
                    }
                }

                var text = expression.ToString();
                foreach (var node in expression.DescendantNodesAndSelf())
                {
                    if (node is LiteralExpressionSyntax literalExpression)
                    {
                        var newValue = Utilities.ConvertOperandToNumericText(literalExpression.Token.Text);

                        text = text.Substring(0, literalExpression.Token.SpanStart - expression.SpanStart) + newValue + text.Substring(literalExpression.Span.End - expression.SpanStart);
                    }
                    else if (node is MemberAccessExpressionSyntax memberAccessExpression)
                    {
                        var newValue = memberAccessExpression.ToString().Replace(".", "::");

                        text = text.Substring(0, memberAccessExpression.SpanStart - expression.SpanStart) + newValue + text.Substring(memberAccessExpression.Span.End - expression.SpanStart);
                    }
                }

                if (expression is LiteralExpressionSyntax || expression is MemberAccessExpressionSyntax || expression is IdentifierNameSyntax)
                    result[i] = text;
                else
                    result[i] = $"({text})";
            }
            catch (InvalidOperationException ex)
            {
                var location = arguments[i].GetLocation();

                if (ex.Message.Contains("binary format"))
                    context.ReportDiagnostic(Diagnostics.InvalidFormat, location, arguments[i].ToString(), "byte with binary format");
                else if (ex.Message.Contains("byte or ushort"))
                    context.ReportDiagnostic(Diagnostics.InvalidFormat, location, arguments[i].ToString(), "byte or ushort");
                else
                    context.ReportDiagnostic(Diagnostics.InternalAnalyzerFailure, location, "ParseOp exception handling failed to match proper message");
            }
        }

        return result;
    }

    private static bool StoreData(string[] dataItems, Location location, MethodVisitorContext context)
    {
        var writer = context.Writer;
        byte index = 0;
        foreach (var data in dataItems)
        {
            if (data == null) continue;
            else if (byte.TryParse(data, out var byteData))
            {
                writer.WriteOpCodeImmediate("lda", byteData);
                writer.WriteOpCode("sta", index++);
            }
            else if (bool.TryParse(data, out var boolData))
            {
                writer.WriteOpCodeImmediate("lda", (byte)(boolData ? 1 : 0));
                writer.WriteOpCode("sta", index++);
            }
            else if (ushort.TryParse(data, out var ushortData))
            {
                writer.WriteOpCodeImmediate("lda", (byte)(ushortData % 256));
                writer.WriteOpCode("sta", index++);
                writer.WriteOpCodeImmediate("lda", (byte)(ushortData / 256));
                writer.WriteOpCode("sta", index++);
            }
            else
            {
                try
                {
                    var value = data.StartsWith("$") 
                        ? Convert.ToInt32($"0x{data.Substring(1)}", 16)
                        : Convert.ToInt32(data, 16);

                    if (value < 256)
                    {
                        writer.WriteOpCodeImmediate("lda", (byte)value);
                        writer.WriteOpCode("sta", index++);
                    }
                    else
                    {
                        writer.WriteOpCodeImmediate("lda", (byte)(value % 256));
                        writer.WriteOpCode("sta", index++);
                        writer.WriteOpCodeImmediate("lda", (byte)(value / 256));
                        writer.WriteOpCode("sta", index++);
                    }
                }
                catch (FormatException)
                {
                    context.ReportDiagnostic(Diagnostics.UnsupportedParameterType2, location, data);
                    continue;
                }
            }
        }

        return true;
    }

    private static void ParseOp(string operation, string operand, string indexorRegister, Location location, ClassVisitorContext context)
    {
        try
        {
            var resolvedOperand = operand.Contains(".") 
                ? operand.Replace(".", "::")
                : context.Consts.Any(c => c == operand)
                    ? operand
                    : Utilities.ConvertOperandToNumericText(operand);

            if (string.IsNullOrEmpty(indexorRegister))
            {
                switch (operation)
                {
                    // TODO Validate LD?i with const should generate error
                    case "LDAi": context.Writer.WriteOpCodeImmediate("lda", resolvedOperand); break;
                    case "LDXi": context.Writer.WriteOpCodeImmediate("ldx", resolvedOperand); break;
                    case "LDYi": context.Writer.WriteOpCodeImmediate("ldy", resolvedOperand); break;
                    case "INC": context.Writer.WriteOpCode("inc", resolvedOperand); break;
                    case "DEC": context.Writer.WriteOpCode("dec", resolvedOperand); break;
                    case "INX": context.Writer.WriteOpCode("inx"); break;
                    case "INY": context.Writer.WriteOpCode("iny"); break;
                    case "DEX": context.Writer.WriteOpCode("dex"); break;
                    case "LDA": context.Writer.WriteOpCode("lda", resolvedOperand); break;
                    case "STA": context.Writer.WriteOpCode("sta", resolvedOperand); break;
                    case "STX": context.Writer.WriteOpCode("stx", resolvedOperand); break;
                    case "CMP": context.Writer.WriteOpCode("cmp", resolvedOperand); break;
                    case "CPXi": context.Writer.WriteOpCodeImmediate("cpx", resolvedOperand); break;
                    case "CPYi": context.Writer.WriteOpCodeImmediate("cpy", resolvedOperand); break;
                    case "LSR": context.Writer.WriteOpCode("lsr a"); break;
                    case "ROL": context.Writer.WriteOpCode("rol", resolvedOperand); break;
                    case "SEI": context.Writer.WriteOpCode("sei"); break;
                    case "CLD": context.Writer.WriteOpCode("cld"); break;
                    case "TXS": context.Writer.WriteOpCode("txs"); break;
                    case "BIT": context.Writer.WriteOpCode("bit", resolvedOperand); break;
                    case "ANDi": context.Writer.WriteOpCodeImmediate("and", resolvedOperand); break;
                    default:
                        {
                            var methodInfo = context.TypeCache.GetMethod(operation, context.AllMethods);
                            if (methodInfo != null)
                            {
                                if (methodInfo.IsProc)
                                    context.Writer.WriteJSROpCode(null, Utilities.GetProcName(operation));
                                else if (methodInfo.IsNoReturnProc)
                                    context.Writer.WriteJMPOpCode(null, Utilities.GetProcName(operation));
                                else if (methodInfo.IsMacro)
                                    context.Writer.WriteCallMacro(null, operation, new[] { resolvedOperand });
                                break;
                            }

                            context.ReportDiagnostic(Diagnostics.InstructionNotSupported, location, operation);
                        }
                        break;
                }
            }
            else
            {
                switch (operation)
                {
                    case "STA": context.Writer.WriteOpCode("sta", resolvedOperand, indexorRegister); break;
                    default:
                        context.ReportDiagnostic(Diagnostics.AbsoluteIndexedOpCodeNotSupported, location, operation);
                        break;
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("binary format"))
                context.ReportDiagnostic(Diagnostics.InvalidFormat, location, operand, "byte with binary format");
            else if (ex.Message.Contains("byte or ushort"))
                context.ReportDiagnostic(Diagnostics.InvalidFormat, location, operand, "byte or ushort");
            else
                context.ReportDiagnostic(Diagnostics.InternalAnalyzerFailure, location, "ParseOp exception handling failed to match proper message");
        }
    }
}