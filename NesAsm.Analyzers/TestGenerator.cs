using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;

namespace NesAsm.Analyzers
{
    [Generator]
    public class TestGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource($"test.g.cs", "using System;");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }

    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            // Do a simple filter for enums
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = initContext.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
                .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

            initContext.RegisterSourceOutput(classDeclarations, (spc, cds) =>
            {
                File.WriteAllText(@"C:\Users\pasca\Dev\GitHub\NesAsm\debug.txt", DateTime.Now.ToString());

                spc.AddSource($"TestInc.{cds.Identifier}.cs", $@"
                    // {cds.BaseList.FullSpan.ToString()}
                    ");
            });
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
            node is ClassDeclarationSyntax m
            && m.BaseList != null;

        static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            foreach (var type in classDeclarationSyntax.BaseList.Types)
            {
                //if (type.Type.ToString() != "ScriptBase")
                //{
                //    continue;
                //}

                if (context.SemanticModel.GetSymbolInfo(type.Type).Symbol is not ITypeSymbol typeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                var fullName = typeSymbol.ToDisplayString();
                File.WriteAllText(@"C:\Users\pasca\Dev\GitHub\NesAsm\debug2.txt", fullName + "TT");

                if (fullName == "NesAsm.Emulator.ScriptBase")
                {
                    return classDeclarationSyntax;
                }
            }

            // we didn't find the attribute we were looking for
            return null;
        }
    }
}
