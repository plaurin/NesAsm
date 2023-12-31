﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal class MethodVisitor
{
    private static readonly Regex opPattern = new("\\s*(?'Operation'\\w+)[((](?'Operand'\\d{0,3}|0x[A-Fa-f0-9]{1,4}|0b[0-1__]*[0-1])[))];", RegexOptions.Compiled);
    private static readonly Regex opXPattern = new("\\s*(Call<(?'Script'\\w+)>.+\\.)?(?'Operation'\\w+)\\s*[((](?'Operands'.*)[))];", RegexOptions.Compiled);
    private static readonly Regex opReturnPattern = new("\\s*var ((?'ReturnValue'\\w+)|([((](?'ReturnValues'.+)[))]))\\s*=\\s*(?'Operation'\\w+)[((](?'Operands'.*)[))];", RegexOptions.Compiled);
   
    private static readonly Regex commentPattern = new("//(?'Comment'.+)", RegexOptions.Compiled);
    private static readonly Regex labelPattern = new("(?'Label'.+):", RegexOptions.Compiled);
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

                if (!ParseOp(match.Groups["Operation"].Value, match.Groups["Operand"].Value, context.AllMethods, writer))
                {
                    throw new InvalidOperationException($"OpCode with return values detected but not supported in {line}");
                }

                continue;
            }

            match = opPattern.Match(line);
            if (match.Success)
            {
                if (!ParseOp(match.Groups["Operation"].Value, match.Groups["Operand"].Value, context.AllMethods, writer))
                {
                    throw new InvalidOperationException($"OpCode detected but not supported in {line}");
                }

                continue;
            }

            match = opXPattern.Match(line);
            if (match.Success)
            {
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
                        context.ReportDiagnostic(Diagnostics.InternalAnalyzerFailure, location, $"Operand {operands.Single()} not supported, should be a method argument in {line}");
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(script))
                {
                    writer.WriteJSROpCode(GetProcName(operation));
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
                        throw new InvalidOperationException($"Operand {operand} not supported for parameter (should be byte, ushort or bool) in {line}");
                    }
                }

                if (operation != "return") writer.WriteJSROpCode(GetProcName(operation));
                continue;
            }

            match = branchPattern.Match(line);
            if (match.Success)
            {
                if (match.Groups["Operation"].Value == "BNE")
                {
                    writer.WriteBranchOpCode("bne", match.Groups["Label"].Value);
                    continue;
                }

                throw new InvalidOperationException($"Branching OpCode detected but not supported in {line}");
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

    private static bool ParseOp(string operation, string operand, string[] allMethods, AsmWriter writer)
    {
        var numericOperand = Utilities.ConvertOperandToNumericText(operand);

        switch (operation)
        {
            case "LDAi": writer.WriteOpCodeImmediate("lda", numericOperand); break;
            case "LDXi": writer.WriteOpCodeImmediate("ldx", numericOperand); break;
            case "INX": writer.WriteOpCode("inx"); break;
            case "STA": writer.WriteOpCode("sta", numericOperand); break;
            case "STX": writer.WriteOpCode("stx", numericOperand); break; 
            case "CPXi": writer.WriteOpCodeImmediate("cpx", numericOperand); break; 
            default:
                {
                    var callingProc = allMethods.SingleOrDefault(m => operation == m);
                    if (callingProc != null)
                    {
                        writer.WriteJSROpCode(GetProcName(operation));
                        return true;
                    }

                    return false;
                }
        }

        return true;
    }

    internal static string GetProcName(MethodDeclarationSyntax method) => GetProcName(method.Identifier.ValueText);
    internal static string GetProcName(string methodName) => $"{char.ToLowerInvariant(methodName[0])}{methodName.Substring(1)}";
}