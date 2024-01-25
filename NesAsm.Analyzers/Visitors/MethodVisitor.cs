using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal class MethodVisitor
{
    private static readonly Regex opPattern = new("\\s*(?'Operation'\\w+)[((](?'Operand'\\d{0,3}|0x[A-Fa-f0-9__]{1,4}|0b[0-1__]*[0-1])[))];", RegexOptions.Compiled);
    private static readonly Regex opXPattern = new("\\s*((?'JumpType'Call|Jump)<(?'Script'\\w+)>.+\\.)?(?'Operation'\\w+)\\s*[((](?'Operands'.*)[))];", RegexOptions.Compiled);
    private static readonly Regex opReturnPattern = new("\\s*var ((?'ReturnValue'\\w+)|([((](?'ReturnValues'.+)[))]))\\s*=\\s*(?'Operation'\\w+)[((](?'Operands'.*)[))];", RegexOptions.Compiled);

    private static readonly Regex commentPattern = new("//(?'Comment'.+)", RegexOptions.Compiled);
    private static readonly Regex labelPattern = new("(?'Label'.+):", RegexOptions.Compiled);
    private static readonly Regex gotoPattern = new("^\\s*goto (?'Label'.+);", RegexOptions.Compiled);
    private static readonly Regex branchPattern = new("if \\((?'Operation'.+)\\(\\)\\) goto (?'Label'.+);", RegexOptions.Compiled);

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    internal static void Visit(MethodDeclarationSyntax method, ClassVisitorContext context)
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

        var lines = method.Body.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            var statement = method.Body.Statements.FirstOrDefault(s => s.ToString().Trim() == line.Trim());
            var location = statement != null ? statement.GetLocation() : Location.None;

            var match = commentPattern.Match(line);
            if (match.Success)
            {
                writer.WriteComment(match.Groups["Comment"].Value);
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
                    if (script != (method.Parent as ClassDeclarationSyntax)?.Identifier.Text)
                        context.AddScriptReference($"{script}.s");

                    if (jumpType == "Call")
                        writer.WriteJSROpCode(Utilities.GetProcName(operation));
                    else if (jumpType == "Jump")
                        writer.WriteJMPOpCode(Utilities.GetProcName(operation));
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
                        context.ReportDiagnostic(Diagnostics.UnsupportedParameterType2, location, operand);
                        continue;
                    }
                }

                if (operation != "return") writer.WriteJSROpCode(Utilities.GetProcName(operation));
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
                StoreData(new[] { operand }, line, writer);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                writer.WriteEmptyLine();
                continue;
            }

            // Ignore
            if (line.Trim() == "{" || line.Trim() == "}") continue;

            throw new InvalidOperationException($"Not supported {line}");
        }

        foreach (var statement in method.Body.Statements)
        {
            if (statement is ExpressionStatementSyntax expression)
            {
            }
        }
    }

    private static bool StoreData(string[] dataItems, string line, AsmWriter writer)
    {
        byte index = 0;
        foreach (var data in dataItems)
        {
            if (byte.TryParse(data, out var byteData))
            {
                writer.WriteOpCodeImmediate("lda", byteData);
                writer.WriteOpCode("sta", index++);
            }
            else
            {
                throw new InvalidOperationException($"Operand {data} not supported for parameter (should be byte, ushort or bool) in {line}");
            }
        }

        return true;
    }

    private static void ParseOp(string operation, string operand, string indexorRegister, Location location, ClassVisitorContext context)
    {
        try
        {
            var numericOperand = Utilities.ConvertOperandToNumericText(operand);
            if (string.IsNullOrEmpty(indexorRegister))
            {
                switch (operation)
                {
                    case "LDAi": context.Writer.WriteOpCodeImmediate("lda", numericOperand); break;
                    case "LDXi": context.Writer.WriteOpCodeImmediate("ldx", numericOperand); break;
                    case "LDYi": context.Writer.WriteOpCodeImmediate("ldy", numericOperand); break;
                    case "INC": context.Writer.WriteOpCode("inc", numericOperand); break;
                    case "DEC": context.Writer.WriteOpCode("dec", numericOperand); break;
                    case "INX": context.Writer.WriteOpCode("inx"); break;
                    case "DEX": context.Writer.WriteOpCode("dex"); break;
                    case "LDA": context.Writer.WriteOpCode("lda", numericOperand); break;
                    case "STA": context.Writer.WriteOpCode("sta", numericOperand); break;
                    case "STX": context.Writer.WriteOpCode("stx", numericOperand); break;
                    case "CMP": context.Writer.WriteOpCode("cmp", numericOperand); break;
                    case "CPXi": context.Writer.WriteOpCodeImmediate("cpx", numericOperand); break;
                    case "LSR": context.Writer.WriteOpCode("lsr a"); break;
                    case "ROL": context.Writer.WriteOpCode("rol", numericOperand); break;
                    case "SEI": context.Writer.WriteOpCode("sei"); break;
                    case "CLD": context.Writer.WriteOpCode("cld"); break;
                    case "TXS": context.Writer.WriteOpCode("txs"); break;
                    case "BIT": context.Writer.WriteOpCode("bit", numericOperand); break;
                    case "ANDi": context.Writer.WriteOpCodeImmediate("and", numericOperand); break;
                    default:
                        {
                            var callingProc = context.AllMethods.SingleOrDefault(m => operation == m);
                            if (callingProc != null)
                            {
                                context.Writer.WriteJSROpCode(Utilities.GetProcName(operation));
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
                    case "STA": context.Writer.WriteOpCode("sta", numericOperand, indexorRegister); break;
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