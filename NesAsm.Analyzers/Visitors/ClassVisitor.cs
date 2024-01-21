using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal static class ClassVisitor
{
    public static void Visit(ClassDeclarationSyntax classDeclarationSyntax, VisitorContext context)
    {
        var writer = context.Writer;

        writer.StartCodeSegment();

        var classVisitorContext = new ClassVisitorContext(context, GetAllClassMethods(classDeclarationSyntax));

        foreach (var member in classDeclarationSyntax.Members)
        {
            MemberVisitor.Visit(member, classVisitorContext);
        }

        foreach (var att in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attribute in att.Attributes)
            {
                if (attribute.Name.ToString() == "PostFileInclude")
                {
                    var filepath = attribute.ArgumentList.Arguments.ToString();
                    writer.IncludeFile(filepath);
                }
            }
        }

        foreach (var scriptReference in classVisitorContext.ScriptReferences)
        {
            writer.IncludeFile($"\"{scriptReference}.s\"");
        }
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