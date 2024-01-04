using ApprovalTests.Reporters;

namespace NesAsm.Analyzers.Tests;

[UseReporter(typeof(VisualStudioReporter))]
public class AsmGeneratorTests
{
    [Fact]
    public void TestSimpleScript()
    {
        TestScript("SimpleScript.cs");
    }

    [Fact]
    public void TestMultiProcScript()
    {
        TestScript("MultiProcScript.cs");
    }

    [Fact]
    public void TestJumpSubroutineScript()
    {
        TestScript("JumpSubroutineScript.cs");
    }

    [Fact]
    public void TestCharsScript()
    {
        TestScript("CharsScript.cs");
    }

    [Fact]
    public void TestBranchScript()
    {
        TestScript("BranchScript.cs");
    }

    [Fact]
    public void TestParametersScript()
    {
        TestScript("ParametersScript.cs");
    }

    private static void TestScript(string filename)
    {
        var generator = new AsmGenerator();
        var content = File.ReadAllText($@"..\..\..\TestFiles\{filename}");

        GeneratorTestUtilities.TestGenerator(generator, content);
    }
}
