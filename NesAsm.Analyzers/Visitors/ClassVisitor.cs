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

        return sb.ToString();
    }
}

internal class MethodVisitor
{
    private static Regex opPattern = new Regex("\\s*(?'Operation'.+)[((](?'Operand'\\d*)[))];", RegexOptions.Compiled);
    private static Regex commentPattern = new Regex("//(?'Comment'.+)", RegexOptions.Compiled);

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    internal static string Visit(MethodDeclarationSyntax method, Compilation compilation, string[] allMethods)
    {
        var sb = new StringBuilder();

        var lines = method.Body.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            var match = commentPattern.Match(line);
            if (match.Success)
            {
                sb.AppendLine($"  ;{match.Groups["Comment"].Value}");
            }

            match = opPattern.Match(line);
            if (match.Success)
            {
                if (match.Groups["Operation"].Value == "LDAi")
                {
                    sb.AppendLine($"  lda #{match.Groups["Operand"].Value}");
                }
                if (match.Groups["Operation"].Value == "LDXi")
                {
                    sb.AppendLine($"  ldx #{match.Groups["Operand"].Value}");
                }
                if (match.Groups["Operation"].Value == "INX")
                {
                    sb.AppendLine($"  inx");
                }

                var callingProc = allMethods.SingleOrDefault(m => match.Groups["Operation"].Value == m);
                if (callingProc != null)
                {
                    sb.AppendLine($"  jsr {GetProcName(callingProc)}");
                }
            }

            if (string.IsNullOrWhiteSpace(line)) sb.AppendLine("");
        }

        foreach (var statement in method.Body.Statements)
        {
            if (statement is ExpressionStatementSyntax expression)
            {
            }
        }

        return sb.ToString();
    }

    internal static string GetProcName(MethodDeclarationSyntax method) => GetProcName(method.Identifier.ValueText);
    internal static string GetProcName(string methodName) => $"{char.ToLowerInvariant(methodName[0])}{methodName.Substring(1)}";
}