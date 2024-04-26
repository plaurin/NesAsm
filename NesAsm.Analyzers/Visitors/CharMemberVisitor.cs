using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers.Visitors;

internal class CharMemberVisitor
{
    internal static void Visit(MemberDeclarationSyntax memberDeclarationSyntax, CharVisitorContext context)
    {
        if (memberDeclarationSyntax is FieldDeclarationSyntax field)
        {
            VisitField(field, context);
        }
    }

    private static void VisitField(FieldDeclarationSyntax field, CharVisitorContext context)
    {
        var fieldName = field.Declaration.Variables.First().Identifier.ToString();
        var fieldType = field.Declaration.Type.ToString();

        if (fieldType == "byte")
        {
            var numericValue = field.Declaration.Variables[0].Initializer?.Value.ToString();
            var asmValue =  Utilities.ConvertOperandToNumericText(numericValue);

            context.AddConst(fieldName, numericValue, asmValue);
        }
    }
}
