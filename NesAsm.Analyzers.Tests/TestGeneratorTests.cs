using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NesAsm.Emulator;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace NesAsm.Analyzers.Tests;

public class TestGeneratorTests
{
    [Fact]
    [UseReporter(typeof(VisualStudioReporter))]
    public void TestGenerator()
    {
        // Create the 'input' compilation that the generator will act on
        Compilation inputCompilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");

        // directly create an instance of the generator
        // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
        var generator = new TestGenerator();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
        Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
        var diag = outputCompilation.GetDiagnostics();
        //Debug.Assert(diag.IsEmpty); // verify the compilation with the added source has no diagnostics

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        // The runResult contains the combined results of all generators passed to the driver
        Debug.Assert(runResult.GeneratedTrees.Length == 1);
        Debug.Assert(runResult.Diagnostics.IsEmpty);

        // Or you can access the individual results on a by-generator basis
        GeneratorRunResult generatorResult = runResult.Results[0];
        Debug.Assert(generatorResult.Generator == generator);
        Debug.Assert(generatorResult.Diagnostics.IsEmpty);
        Debug.Assert(generatorResult.GeneratedSources.Length == 1);
        Debug.Assert(generatorResult.Exception is null);

        Approvals.Verify(generatorResult.GeneratedSources[0].SourceText);
    }

    [Fact]
    [UseReporter(typeof(VisualStudioReporter))]
    public void TestIncrementalGenerator()
    {
        // Create the 'input' compilation that the generator will act on
        Compilation inputCompilation = CreateCompilation(@"
using NesAsm.Emulator;
namespace MyCode;
public class Program : NesAsm.Emulator.ScriptBase
{
    public Program(NESEmulator emulator) : base(emulator) {}
}
");

        // directly create an instance of the generator
        // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
        var generator = new Generator();

        // Create the driver that will control the generation, passing in our generator
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        var sb = new StringBuilder();

        sb.AppendLine("- Main diagnostics:");
        foreach (var diagnostic in diagnostics)
        {
            sb.AppendLine(diagnostic.GetMessage());
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
            sb.AppendLine(diag.GetMessage());
        }

        // Or we can look at the results directly:
        GeneratorDriverRunResult runResult = driver.GetRunResult();

        // The runResult contains the combined results of all generators passed to the driver
        //Debug.Assert(runResult.GeneratedTrees.Length == 1);
        sb.AppendLine($"- RunResult.GeneratedTrees Count: {runResult.GeneratedTrees.Length}");

        //Debug.Assert(runResult.Diagnostics.IsEmpty);
        sb.AppendLine("- RunResults diagnostics:");
        foreach (var diag in runResult.Diagnostics)
        {
            sb.AppendLine(diag.GetMessage());
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
                sb.AppendLine(diag.GetMessage());
            }

            //Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            sb.AppendLine($"- RunResult[{i}].GeneratedSources Count: {generatorResult.GeneratedSources.Length}");

            //Debug.Assert(generatorResult.Exception is null);
            sb.AppendLine($"- RunResult[{i}].Exception: {generatorResult.Exception}");

            var j = 0;
            foreach (var generatedSource in generatorResult.GeneratedSources)
            {
                sb.AppendLine($"- RunResult[{i}].GeneratedSources[{j}].SourceText:");
                sb.AppendLine($"{generatedSource.SourceText}");
                j++;
            }

            i++;
        }


        Approvals.Verify(sb.ToString());
    }

    private static Compilation CreateCompilation(string source)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ScriptBase).GetTypeInfo().Assembly.Location),
        };

        Assembly.GetEntryAssembly().GetReferencedAssemblies()
            .ToList()
            .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

        return CSharpCompilation.Create("compilation",
                    new[] { CSharpSyntaxTree.ParseText(source) },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}