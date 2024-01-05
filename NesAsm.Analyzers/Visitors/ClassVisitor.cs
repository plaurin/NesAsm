using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;
internal static class ClassVisitor
{
    public static string Visit(ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation)
    {
        var sb = new StringBuilder();

        sb.AppendLine(".segment \"CODE\"");
        sb.AppendLine("");

        var allMethods = GetAllClassMethods(classDeclarationSyntax);

        foreach (var member in classDeclarationSyntax.Members)
        {
            sb.Append(MemberVisitor.Visit(member, compilation, allMethods));
        }

        foreach (var att in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attribute in att.Attributes)
            {
                if (attribute.Name.ToString() == "PostFileInclude")
                {
                    var filepath = attribute.ArgumentList.Arguments.ToString();
                    sb.AppendLine($".include {filepath}");
                }
            }
        }
        return sb.ToString();
    }

    private static string[] GetAllClassMethods(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var result = new List<string>();

        foreach (var member in classDeclarationSyntax.Members)
        {
            if (member is MethodDeclarationSyntax method)
            {
                result.Add(method.Identifier.ValueText);
            }

        }

        return result.ToArray();
    }
}

internal class MemberVisitor
{
    internal static string Visit(MemberDeclarationSyntax member, Compilation compilation, string[] allMethods)
    {
        var sb = new StringBuilder();

        if (member is MethodDeclarationSyntax method)
        {
            sb.AppendLine($".proc {MethodVisitor.GetProcName(method)}");

            sb.Append(MethodVisitor.Visit(method, compilation, allMethods));

            sb.AppendLine("");
            sb.AppendLine("  rts");
            sb.AppendLine(".endproc");
            sb.AppendLine("");
        }


        var charBytes = new List<int>();
        if (member is FieldDeclarationSyntax field)
        {
            if (field.Declaration.Type.ToString() == "byte[]")
            {
                foreach (var element in (field.Declaration.Variables[0].Initializer.Value as CollectionExpressionSyntax).Elements)
                {
                    charBytes.Add((int)((element as ExpressionElementSyntax).Expression as LiteralExpressionSyntax).Token.Value);
                }
            }
        }

        if (charBytes.Any())
        {
            sb.AppendLine(".segment \"CHARS\"");
            sb.AppendLine("");

            int i = 0;
            foreach (var charByte in charBytes)
            {
                sb.AppendLine($"  .byte %{charByte:B8}");

                if (++i % 8 == 0) sb.AppendLine("");
            }
        }

        return sb.ToString();
    }
}

internal class MethodVisitor
{
    private static Regex opPattern = new Regex("\\s*(?'Operation'\\w+)[((](?'Operand'\\d{0,3}|0x[A-Fa-f0-9]{1,4}|0b[0-1__]*[0-1])[))];", RegexOptions.Compiled);
    private static Regex opXPattern = new Regex("\\s*(?'Operation'\\w+)\\s*[((](?'Operands'.*)[))];", RegexOptions.Compiled);
    private static Regex opReturnPattern = new Regex("\\s*var ((?'ReturnValue'\\w+)|([((](?'ReturnValues'.+)[))]))\\s*=\\s*(?'Operation'\\w+)[((](?'Operands'.*)[))];", RegexOptions.Compiled);
   
    private static Regex commentPattern = new Regex("//(?'Comment'.+)", RegexOptions.Compiled);
    private static Regex labelPattern = new Regex("(?'Label'.+):", RegexOptions.Compiled);
    private static Regex branchPattern = new Regex("if \\((?'Operation'.+)\\(\\)\\) goto (?'Label'.+);", RegexOptions.Compiled);

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    internal static string Visit(MethodDeclarationSyntax method, Compilation compilation, string[] allMethods)
    {
        var sb = new StringBuilder();

        var parameters = new List<(int index, string identifier, string type)>();
        var paramIndex = 0;
        foreach (var parameter in method.ParameterList.Parameters)
        {
            parameters.Add((paramIndex++, parameter.Identifier.ToString(), parameter.Type.ToString()));
        }

        var lines = method.Body.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            var match = commentPattern.Match(line);
            if (match.Success)
            {
                sb.AppendLine($"  ;{match.Groups["Comment"].Value}");
                continue;
            }

            match = labelPattern.Match(line);
            if (match.Success)
            {
                sb.AppendLine($"@{match.Groups["Label"].Value.Trim()}:");
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

                if (!ParseOp(match.Groups["Operation"].Value, match.Groups["Operand"].Value, sb, allMethods, line))
                {
                    throw new InvalidOperationException($"OpCode with return values detected but not supported in {line}");
                }

                continue;
            }

            match = opPattern.Match(line);
            if (match.Success)
            {
                if (!ParseOp(match.Groups["Operation"].Value, match.Groups["Operand"].Value, sb, allMethods, line))
                {
                    throw new InvalidOperationException($"OpCode detected but not supported in {line}");
                }

                continue;
            }

            match = opXPattern.Match(line);
            if (match.Success)
            {
                var operation = match.Groups["Operation"].Value;
                var operands = match.Groups["Operands"].Value.Split(',').Select(o => o.Trim());

                if (operands.Count() == 1 && (operation == "LDAa" || operation == "LDXa" || operation == "LDYa"))
                {
                    var parameter = parameters.SingleOrDefault(p => p.identifier == operands.Single());
                    if (parameter.identifier != null)
                    {
                        var opCode = operation switch
                        {
                            "LDAa" => "lda",
                            "LDXa" => "ldx",
                            "LDYa" => "ldy",
                            _ => throw new InvalidOperationException($"OpCode {operation} not supported to use arguments (should be LDAa, LDXa or LDYa) in {line}")
                        };

                        switch (parameter.type)
                        {
                            case "bool": sb.AppendLine($"  {opCode} ${parameter.index}"); break;
                            case "byte": sb.AppendLine($"  {opCode} ${parameter.index}"); break;
                            case "ushort": throw new NotImplementedException("ushort arguments not implemented yet");
                            default: throw new InvalidOperationException($"Argument type {parameter.type} not supported (should be byte, ushort or bool) in {line}");
                        }

                        continue;
                    }

                    throw new InvalidOperationException($"Operand {operands.Single()} not supported, should be a method argument in {line}");
                }

                // TODO Use StoreData method instead
                var index = 0;
                foreach (var operand in operands)
                {
                    if (bool.TryParse(operand, out var boolOperand))
                    {
                        sb.AppendLine($"  lda #{(boolOperand ? 1 : 0)}");
                        sb.AppendLine($"  sta ${index++}");
                    }
                    else if (byte.TryParse(operand, out var byteOperand))
                    {
                        sb.AppendLine($"  lda #{byteOperand}");
                        sb.AppendLine($"  sta ${index++}");
                    }
                    else if (ushort.TryParse(operand, out var ushortOperand))
                    {
                        sb.AppendLine($"  lda #{ushortOperand % 256}");
                        sb.AppendLine($"  sta ${index++}");
                        sb.AppendLine($"  lda #{ushortOperand / 256}");
                        sb.AppendLine($"  sta ${index++}");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Operand {operand} not supported for parameter (should be byte, ushort or bool) in {line}");
                    }
                }

                if (operation != "return") sb.AppendLine($"  jsr {GetProcName(operation)}");
                continue;
            }

            match = branchPattern.Match(line);
            if (match.Success)
            {
                if (match.Groups["Operation"].Value == "BNE")
                {
                    sb.AppendLine($"  bne @{match.Groups["Label"].Value}");
                    continue;
                }

                throw new InvalidOperationException($"Branching OpCode detected but not supported in {line}");
            }

            if (line.Trim().StartsWith("return"))
            {
                var operand = line.Trim().Substring(7).TrimEnd(';').Trim();
                StoreData(new[] { operand }, sb, line);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine("");
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

        return sb.ToString();
    }

    private static bool StoreData(string[] dataItems, StringBuilder sb, string line)
    {
        var index = 0;
        foreach (var data in dataItems)
        {
            if (byte.TryParse(data, out var byteData))
            {
                sb.AppendLine($"  lda #{byteData}");
                sb.AppendLine($"  sta ${index++}");
            }
            else
            {
                throw new InvalidOperationException($"Operand {data} not supported for parameter (should be byte, ushort or bool) in {line}");
            }
        }

        return true;
    }

    private static bool ParseOpWithReturnValues(string operation, string operand, StringBuilder sb, string[] allMethods, string line)
    {

        return true;
    }

    private static bool ParseOp(string operation, string operand, StringBuilder sb, string[] allMethods, string line)
    {
        var numericOperand = Utilities.ConvertOperandToNumericText(operand);

        switch (operation)
        {
            case "LDAi": sb.AppendLine($"  lda #{numericOperand}"); break;
            case "LDXi": sb.AppendLine($"  ldx #{numericOperand}"); break;
            case "INX": sb.AppendLine($"  inx");break;
            case "STA": sb.AppendLine($"  sta {numericOperand}"); break;
            case "STX": sb.AppendLine($"  stx {numericOperand}"); break;
            case "CPXi": sb.AppendLine($"  cpx #{numericOperand}"); break;
            default:
                {
                    var callingProc = allMethods.SingleOrDefault(m => operation == m);
                    if (callingProc != null)
                    {
                        sb.AppendLine($"  jsr {GetProcName(operation)}");
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