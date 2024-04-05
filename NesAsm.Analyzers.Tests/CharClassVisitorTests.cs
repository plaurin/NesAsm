using ApprovalTests;
using ApprovalTests.Reporters;
using NesAsm.Analyzers.Visitors;

namespace NesAsm.Analyzers.Tests;

[UseReporter(typeof(VisualStudioReporter))]
public class CharClassVisitorTests
{
    [Fact]
    public void Test()
    {
        var result = CharClassVisitor.Process(@"C:\Users\pasca\Dev\GitHub\NesAsm\NesAsm.Example\game1.png");

        Approvals.Verify(result);
    }

    [Fact]
    public void TestImportCharScript()
    {
        TestScript("ImportCharDefinition.cs");
    }

    private static void TestScript(params string[] filenames)
    {
        var generator = new CharGenerator();

        var sourcePaths = filenames.Select(filename => $@"..\..\..\TestFiles\{filename}").ToArray();

        GeneratorTestUtilities.TestGenerator(generator, sourcePaths);
    }
}