using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers
{
    [Generator]
    public class AsmGenerator : IIncrementalGenerator
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
                spc.AddSource($"Asm.{cds.Identifier}.cs", $@"
                    // {cds.BaseList.FullSpan}
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
                if (context.SemanticModel.GetSymbolInfo(type.Type).Symbol is not ITypeSymbol typeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                var fullName = typeSymbol.ToDisplayString();
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
