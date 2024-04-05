using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NesAsm.Analyzers.Visitors;

namespace NesAsm.Analyzers;

[Generator]
public class AsmGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProviders = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), 
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) 
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(classProviders, Generate);
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

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    private void Generate(SourceProductionContext context, (ClassDeclarationSyntax, Compilation) tuple)
    {
        var (node, compilation) = tuple;
        if (node == null) return;

        try
        {
            var writer = new AsmWriter();

            ClassVisitor.Visit(node, new VisitorContext(context, compilation, writer));

            var source = writer.ToString();

            context.AddSource($"Asm.{node.Identifier}.cs", $@"/* Generator Asm code in file Asm.{node.Identifier}.s
{source}
*/");
            File.WriteAllText($@"C:\Users\pasca\Dev\GitHub\NesAsm\NesAsm.Example\Output\{node.Identifier}.s", source);
        }
        catch (Exception ex)
        {
            var message = $"{nameof(CharGenerator)} for {node.SyntaxTree?.FilePath} : {ex.Message} {ex.StackTrace}";

            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InternalAnalyzerFailure, node.GetLocation(), message));
        }
    }
}
