using ApprovalTests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NesAsm.Emulator;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace NesAsm.Analyzers.Tests;

internal class GeneratorTestUtilities
{
    public static void TestGenerator(ISourceGenerator generator, string source) => TestGenerator(generator, new[] { source });

    public static void TestGenerator(ISourceGenerator generator, string[] sources)
    {
        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        TestGenerator(driver, sources);
    }

    public static void TestGenerator(IIncrementalGenerator generator, string source) => TestGenerator(generator, new[] { source });

    public static void TestGenerator(IIncrementalGenerator generator, string[] sources)
    {
        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        TestGenerator(driver, sources);
    }

    private static void TestGenerator(GeneratorDriver driver, string[] sources)
    {
        Compilation inputCompilation = CreateCompilation(sources);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);
        var runResult = driver.GetRunResult();

        var summary = SummarizeGeneratorResults(diagnostics, outputCompilation, runResult);

        Approvals.Verify(summary);
    }

    private static Compilation CreateCompilation(string[] sources)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(NESEmulator).GetTypeInfo().Assembly.Location),
        };

        Assembly.GetEntryAssembly().GetReferencedAssemblies()
            .ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

        return CSharpCompilation.Create("compilation",
                    sources.Select(source => CSharpSyntaxTree.ParseText(source)),
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static string SummarizeGeneratorResults(ImmutableArray<Diagnostic> diagnostics, Compilation outputCompilation, GeneratorDriverRunResult runResult)
    {
        var sb = new StringBuilder();

        sb.AppendLine("- Main diagnostics:");
        foreach (var diagnostic in diagnostics)
        {
            sb.AppendLine(diagnostic.ToString());
        }

        sb.AppendLine($"- SyntaxTrees Count: {outputCompilation.SyntaxTrees.Count()}");

        // We can now assert things about the resulting compilation:
        //Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
        //Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
        var diags = outputCompilation.GetDiagnostics();
        //Debug.Assert(diag.IsEmpty); // verify the compilation with the added source has no diagnostics

        sb.AppendLine("- Output diagnostics:");
        foreach (var diag in diags)
        {
            sb.AppendLine(diag.ToString());
        }

        // Or we can look at the results directly:

        // The runResult contains the combined results of all generators passed to the driver
        //Debug.Assert(runResult.GeneratedTrees.Length == 1);
        sb.AppendLine($"- RunResult.GeneratedTrees Count: {runResult.GeneratedTrees.Length}");

        //Debug.Assert(runResult.Diagnostics.IsEmpty);
        sb.AppendLine("- RunResults diagnostics:");
        foreach (var diag in runResult.Diagnostics)
        {
            sb.AppendLine(diag.ToString());
        }

        // Or you can access the individual results on a by-generator basis
        var i = 0;
        foreach (var generatorResult in runResult.Results)
        {
            //GeneratorRunResult generatorResult = runResult.Results[0];
            //Debug.Assert(generatorResult.Generator == generator);
            sb.AppendLine($"- RunResult[{i}].Generator: {generatorResult.Generator}");

            //Debug.Assert(generatorResult.Diagnostics.IsEmpty);
            sb.AppendLine($"- RunResult[{i}] diagnostics:");
            foreach (var diag in generatorResult.Diagnostics)
            {
                sb.AppendLine(diag.ToString());
            }

            //Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            sb.AppendLine($"- RunResult[{i}].GeneratedSources Count: {generatorResult.GeneratedSources.Length}");

            //Debug.Assert(generatorResult.Exception is null);
            sb.AppendLine($"- RunResult[{i}].Exception: {generatorResult.Exception}");

            var j = 0;
            foreach (var generatedSource in generatorResult.GeneratedSources)
            {
                sb.AppendLine($"- RunResult[{i}].GeneratedSources[{j}].HintName: {generatedSource.HintName}");
                sb.AppendLine($"- RunResult[{i}].GeneratedSources[{j}].SourceText:");
                sb.AppendLine($"{generatedSource.SourceText}");
                j++;
            }

            i++;
        }

        return sb.ToString();
    }
}
