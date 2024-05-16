using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal static class ClassVisitor
{
    public static void Visit(ClassDeclarationSyntax classDeclarationSyntax, VisitorContext context)
    {
        var classVisitorContext = new ClassVisitorContext(context, GetAllClassMethods(classDeclarationSyntax));

        context.TypeCache.Scan(classDeclarationSyntax);

        ProcessClassAttributes(classDeclarationSyntax, classVisitorContext);

        if (classVisitorContext.IsStartupClass)
            classVisitorContext.Writer.StartCodeSegment();
        else
            classVisitorContext.Writer.StartClassScope(classDeclarationSyntax.Identifier.ToString());

        foreach (var member in classDeclarationSyntax.Members)
        {
            MemberVisitor.Visit(member, classVisitorContext);
        }

        if (!classVisitorContext.IsStartupClass)
            classVisitorContext.Writer.EndClassScope();

        foreach (var scriptReference in classVisitorContext.ScriptReferences)
        {
            classVisitorContext.Writer.IncludeFile(scriptReference);
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

    private static void ProcessClassAttributes(ClassDeclarationSyntax classDeclarationSyntax, ClassVisitorContext context)
    {
        foreach (var att in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attribute in att.Attributes)
            {
                if (attribute.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.Identifier.Text == "FileInclude")
                {
                    var filepath = genericNameSyntax.TypeArgumentList.Arguments.ToString().Trim('"');
                    var relativePath = (attribute.ArgumentList?.Arguments!.FirstOrDefault()?.Expression as LiteralExpressionSyntax)?.Token.ValueText;
                    context.AddPreScriptReference(filepath, relativePath);
                }

                if (attribute.Name.ToString() == "PostFileInclude")
                {
                    var filepath = attribute.ArgumentList.Arguments.ToString().Trim('"');
                    context.AddScriptReference(filepath);
                }

                if (attribute.Name.ToString() == "HeaderSegment")
                {
                    byte prgRomBanks = 2;
                    byte chrRomBanks = 1;
                    byte mapper = 1;
                    bool verticalMirroring = true;
                    var paramIndex = 0;

                    foreach (var argument in attribute.ArgumentList.Arguments)
                    {
                        var namedParameter = argument.NameColon?.Expression.ToString();
                        var argumentValue = argument.Expression.ToString();

                        if (namedParameter == nameof(prgRomBanks))
                            prgRomBanks = byte.Parse(argumentValue);
                        else if (namedParameter == nameof(chrRomBanks))
                            chrRomBanks = byte.Parse(argumentValue);
                        else if (namedParameter == nameof(mapper))
                            mapper = byte.Parse(argumentValue);
                        else if (namedParameter == nameof(verticalMirroring))
                            verticalMirroring = bool.Parse(argumentValue);
                        else if (paramIndex == 0)
                            prgRomBanks = byte.Parse(argumentValue);
                        else if (paramIndex == 1)
                            chrRomBanks = byte.Parse(argumentValue);
                        else if (paramIndex == 2)
                            mapper = byte.Parse(argumentValue);
                        else if (paramIndex == 3)
                            verticalMirroring = bool.Parse(argumentValue);

                        paramIndex++;
                    }

                    context.Writer.StartHeaderSegment(prgRomBanks, chrRomBanks, mapper, verticalMirroring);
                    context.SetStartupClass();
                }

                if (attribute.Name.ToString() == "VectorsSegment")
                {
                    context.Writer.StartVectorsSegment();
                }

                if (attribute.Name.ToString() == "StartupSegment")
                {
                    context.Writer.StartStartupSegment();
                }
            }
        }
    }
}