using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal class MemberVisitor
{
    internal static void Visit(MemberDeclarationSyntax memberDeclarationSyntax, ClassVisitorContext context)
    {
        var writer = context.Writer;

        if (memberDeclarationSyntax is MethodDeclarationSyntax method)
        {
            var isNmi = method.Identifier.ValueText.ToLowerInvariant() == "nmi";

            if (isNmi)
                writer.StartNmi();
            else
                writer.StartProc(Utilities.GetProcName(method));

            MethodVisitor.Visit(method, new MethodVisitorContext(context));

            if (isNmi)
                writer.EndNmi();
            else
                writer.EndProc();
        }

        if (memberDeclarationSyntax is FieldDeclarationSyntax field)
        {
            VisitField(field, context);
        }

    }

    private static void VisitField(FieldDeclarationSyntax field, ClassVisitorContext context)
    {
        var fieldName = field.Declaration.Variables.First().Identifier.ToString();
        var fieldType = field.Declaration.Type.ToString();

        var charBytes = new List<int>();
        if (fieldType == "byte[]")
        {
            foreach (var element in (field.Declaration.Variables[0].Initializer.Value as CollectionExpressionSyntax).Elements)
            {
                charBytes.Add((int)((element as ExpressionElementSyntax).Expression as LiteralExpressionSyntax).Token.Value);
            }
        }
        else if (fieldType == "ushort" || fieldType == "byte")
        {
            var value = field.Declaration.Variables[0].Initializer?.Value.ToString();
            var asmValue =  Utilities.ConvertOperandToNumericText(value);

            context.Writer.WriteConstant(Utilities.GetConstName(fieldName), asmValue);
            context.AddConst(fieldName);
        }

        if (charBytes.Any())
        {
            var dataType = new List<string>();
            foreach (var att in field.AttributeLists)
            {
                foreach (var attribute in att.Attributes)
                {
                    if (attribute.Name.ToString() == "CharData")
                        dataType.Add("CharData");

                    if (attribute.Name.ToString() == "RomData")
                        dataType.Add("RomData");
                }
            }

            if (!dataType.Any())
            {
                context.ReportDiagnostic(Diagnostics.MissingFieldDataType, field.GetLocation());
                return;
            }

            if (dataType.Any(dt => dt == "RomData") && dataType.Any(dt => dt == "CharData"))
            {
                context.ReportDiagnostic(Diagnostics.ManyFieldDataType, field.GetLocation());
                return;
            }

            if (dataType.Any(dt => dt == "CharData"))
            {
                context.Writer.StartCharsSegment();
            }

            if (dataType.Any(dt => dt == "RomData"))
            {
                context.Writer.StartCodeSegment();
                context.Writer.WriteVariableLabel(Utilities.GetLabelName(fieldName));
            }

            context.Writer.WriteChars(charBytes.ToArray());
        }
    }
}
