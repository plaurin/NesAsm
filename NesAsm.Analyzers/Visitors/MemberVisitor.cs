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
            if (method.Modifiers.Any(m => m.ValueText == "static"))
            {
                var isNmi = method.Identifier.ValueText.ToLowerInvariant() == "nmi";
                var isMacro = method.AttributeLists.Any(att => att.Attributes.Any(a => a.Name.ToString() == "Macro"));

                if (isNmi)
                    writer.StartNmi();
                else if (isMacro)
                {
                    var paramNames = method.ParameterList.Parameters.Select(p => p.Identifier.ValueText).ToArray();
                    writer.StartMacro(Utilities.GetMacroName(method), paramNames);
                }
                else
                    writer.StartProc(Utilities.GetProcName(method));

                MethodVisitor.Visit(method, new MethodVisitorContext(context));

                if (isNmi)
                    writer.EndNmi();
                else if (isMacro)
                    writer.EndMacro();
                else
                    writer.EndProc();
            }
            else
            {
                context.ReportDiagnostic(Diagnostics.MethodNotStatic, method.GetLocation());
            }
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
            if (field.Declaration.Variables[0].Initializer.Value is CollectionExpressionSyntax collectionExpression)
            {
                foreach (var element in collectionExpression.Elements)
                {
                    charBytes.Add((int)((element as ExpressionElementSyntax).Expression as LiteralExpressionSyntax).Token.Value);
                }
            }
            else if (field.Declaration.Variables[0].Initializer.Value is InvocationExpressionSyntax invocationExpression)
            {
                if ((invocationExpression.Expression is IdentifierNameSyntax identifierName && identifierName.Identifier.ValueText == "GenerateSpriteData")
                    || (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression && memberAccessExpression.Name.Identifier.ValueText == "GenerateSpriteData"))
                {
                    var names = new string[] { "x", "y", "tileIndex", "paletteIndex" };
                    int argIndex = 0;

                    var args = invocationExpression.ArgumentList.Arguments.ToDictionary(
                        a => a.NameColon?.Name?.Identifier.Value ?? names[argIndex++],
                        a => (a.Expression as LiteralExpressionSyntax).Token.Text);

                    void ParseValue(string key)
                    {
                        args.TryGetValue(key, out var valueText);
                        valueText ??= "0";
                        charBytes.Add(byte.Parse(valueText));
                    }

                    ParseValue("y");
                    ParseValue("tileIndex");
                    ParseValue("paletteIndex");
                    ParseValue("x");
                }
            }
            // Else should warn we don't know what to do
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
