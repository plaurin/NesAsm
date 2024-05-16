using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NesAsm.Analyzers.Visitors;

namespace NesAsm.Analyzers;

[Generator]
public class AsmGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var outputFolder = context.AnalyzerConfigOptionsProvider
            .Select((options, _) =>
            {
                options.GlobalOptions.TryGetValue("nesasm_output", out var outputFolder);

                return outputFolder;
            });

        var classProviders = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), 
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) 
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(classProviders.Combine(outputFolder), Generate);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax.BaseList != null)
            {
                foreach (var type in classDeclarationSyntax.BaseList.Types)
                {
                    if (type.Type is IdentifierNameSyntax identifier)
                    {
                        if (identifier.Identifier.Text == "NesScript")
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

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
            if (fullName == "NesAsm.Emulator.NesScript")
            {
                return classDeclarationSyntax;
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
    private void Generate(SourceProductionContext context, ((ClassDeclarationSyntax, Compilation), string? OutputPath) tuple)
    {
        var ((node, compilation), outputPath) = tuple;
        if (node == null) return;

        // TODO fix .editorconfig
        //outputPath ??= "Output";

        try
        {
            var fullPath = Utilities.GetOutputFolder(Environment.CurrentDirectory, node.SyntaxTree.FilePath, outputPath);
            Directory.CreateDirectory(fullPath);

            var writer = new AsmWriter();

            ClassVisitor.Visit(node, new VisitorContext(context, compilation, writer));

            var source = writer.ToString();

            context.AddSource($"Asm.{node.Identifier}.cs", $@"/* Generator Asm code in file Asm.{node.Identifier}.s
{source}
*/");

            File.WriteAllText($"{Path.Combine(fullPath, node.Identifier.Text)}.s", source);
        }
        catch (Exception ex)
        {
            var message = $"{nameof(CharGenerator)} for {node.SyntaxTree?.FilePath} : {ex.Message} {ex.StackTrace}";

            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InternalAnalyzerFailure, node.GetLocation(), message));
        }
    }
}
