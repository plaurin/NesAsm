﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NesAsm.Analyzers.Visitors;

namespace NesAsm.Analyzers;

[Generator]
public class CharGenerator : IIncrementalGenerator
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
            var hasBaseClass = false;

            if (classDeclarationSyntax.BaseList != null)
            {
                foreach (var type in classDeclarationSyntax.BaseList.Types)
                {
                    if (type.Type is IdentifierNameSyntax identifier)
                    {
                        if (identifier.Identifier.Text == "CharDefinition")
                        {
                            hasBaseClass = true;
                            break;
                        }
                    }
                }
            }

            if (hasBaseClass)
            {
                foreach (var att in classDeclarationSyntax.AttributeLists)
                {
                    foreach (var attribute in att.Attributes)
                    {
                        if (attribute.Name.ToString() == "ImportChar")
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
            if (fullName == "NesAsm.Emulator.CharDefinition")
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
            var asmWriter = new AsmWriter();
            var csWriter = new CsWriter();

            CharClassVisitor.Visit(node, new VisitorContext(context, compilation, asmWriter, csWriter));

            var fullPath = Utilities.GetOutputFolder(Environment.CurrentDirectory, node.SyntaxTree.FilePath, outputPath);
            Directory.CreateDirectory(fullPath);

            // Asm file output
            var source = asmWriter.ToString();
            context.AddSource($"Asm.{node.Identifier}.cs", $@"/* Generator Asm code in file Asm.{node.Identifier}.s
{source}
*/");
            File.WriteAllText($"{Path.Combine(fullPath, node.Identifier.Text)}.s", source);

            // Cs file output
            var csSource = csWriter.ToString();
            context.AddSource($"Sharp.{node.Identifier}.cs", csSource);

            File.WriteAllText($"{Path.Combine(fullPath, node.Identifier.Text)}.csharp", csSource);
        }
        catch (Exception ex)
        {
            var message = $"{nameof(CharGenerator)} for {node.SyntaxTree?.FilePath} : {ex.Message} {ex.StackTrace}";

            context.ReportDiagnostic(Diagnostic.Create(CharDiagnostics.InternalAnalyzerFailure, node.GetLocation(), message));
        }
    }
}
