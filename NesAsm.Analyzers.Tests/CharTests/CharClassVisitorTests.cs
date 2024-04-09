using ApprovalTests.Reporters;

namespace NesAsm.Analyzers.Tests.CharTests;

[UseReporter(typeof(VisualStudioReporter))]
public class CharClassVisitorTests
{
    [Fact]
    public void TestImportCharScript()
    {
        TestScript("ImportCharDefinition.cs");
    }

    private static void TestScript(params string[] filenames)
    {
        var generator = new CharGenerator();

        var sourcePaths = filenames.Select(filename => $@"..\..\..\CharTests\TestFiles\{filename}").ToArray();

        GeneratorTestUtilities.TestGenerator(generator, sourcePaths);
    }
}