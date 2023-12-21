using ApprovalTests.Reporters;

namespace NesAsm.Analyzers.Tests;

public class AsmGeneratorTests
{
    [Fact]
    [UseReporter(typeof(VisualStudioReporter))]
    public void TestSimpleScript()
    {
        var generator = new AsmGenerator();
        var content = File.ReadAllText(@"..\..\..\TestFiles\SimpleScript.cs");

        GeneratorTestUtilities.TestGenerator(generator, content);
    }
}
