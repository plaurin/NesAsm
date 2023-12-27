using System;
using System.Diagnostics.CodeAnalysis;
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

        foreach (var member in classDeclarationSyntax.Members)
        {
            sb.Append(MemberVisitor.Visit(member, compilation));
        }

        foreach (var att in classDeclarationSyntax.AttributeLists)
        { 
            foreach (var attribute in att.Attributes)
            {
                if (attribute.Name.ToString() == "PostFileInclude")
                {
                    var filepath = attribute.ArgumentList.Arguments.ToString();
                    sb.AppendLine("");
                    sb.AppendLine($".include {filepath}");
                }
            }
        }
        return sb.ToString();
    }
}

internal class MemberVisitor
{
    internal static string Visit(MemberDeclarationSyntax member, Compilation compilation)
    {
        var sb = new StringBuilder();

        if (member is MethodDeclarationSyntax method)
        {
            sb.AppendLine($".proc {method.Identifier.ValueText.ToLowerInvariant()}");

            sb.Append(MethodVisitor.Visit(method, compilation));

            sb.AppendLine("");
            sb.AppendLine("  rts");
            sb.AppendLine(".endproc");
        }

        return sb.ToString();
    }
}

internal class MethodVisitor
{
    private static Regex opPattern = new Regex("\\s*(?'Operation'.+)[((](?'Operand'\\d*)[))];", RegexOptions.Compiled);
    private static Regex commentPattern = new Regex("//(?'Comment'.+)", RegexOptions.Compiled);

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    internal static string Visit(MethodDeclarationSyntax method, Compilation compilation)
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
                if (match.Groups["Operation"].Value == "LDXi")
                {
                    sb.AppendLine($"  ldx #{match.Groups["Operand"].Value}");
                }
                if (match.Groups["Operation"].Value == "INX")
                {
                    sb.AppendLine($"  inx");
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
}