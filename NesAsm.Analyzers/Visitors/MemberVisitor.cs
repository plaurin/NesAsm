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
            writer.StartProc(MethodVisitor.GetProcName(method));

            MethodVisitor.Visit(method, context);

            writer.EndProc();
        }

        var charBytes = new List<int>();
        if (memberDeclarationSyntax is FieldDeclarationSyntax field)
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
            writer.StartCharsSegment();

            writer.WriteChars(charBytes.ToArray());
        }
    }
}
