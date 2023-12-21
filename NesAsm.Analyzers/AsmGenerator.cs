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
                var source = $@".segment ""CODE""

.proc Main
  ; Start by loading the value 25 into the Accumulator register
  lda #25

  ; Increment the value of the Accumulator registrer
  inc
";

                spc.AddSource($"Asm.{cds.Identifier}.cs", $@"/* Generator Asm code in file Asm.{cds.Identifier}.s
{source}
*/");

                // TODO output .s file too
            });
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
            node is ClassDeclarationSyntax m
            && m.BaseList != null;

        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
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
