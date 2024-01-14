using ApprovalTests.Reporters;

namespace NesAsm.Analyzers.Tests;

[UseReporter(typeof(VisualStudioReporter))]
public class AsmGeneratorTests
{
    [Fact]
    public void TestExampleScript()
    {
        var generator = new AsmGenerator();
        var content = File.ReadAllText($@"..\..\..\..\NesAsm.Example\HiloWorld.cs");

        GeneratorTestUtilities.TestGenerator(generator, content);
    }

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

    [Fact]
    public void TestSubroutineResultScript()
    {
        TestScript("SubroutineResultScript.cs");
    }

    [Fact]
    public void TestSubroutineResultsScript()
    {
        TestScript("SubroutineResultsScript.cs");
    }

    [Fact]
    public void TestCallingOtherScript()
    {
        TestScript("CallingOtherScript.cs");
    }

    [Fact]
    public void TestDataScript()
    {
        TestScript("DataScript.cs");
    }

    [Fact]
    public void TestAllInstructionsScript()
    {
        TestScript("AllInstructionsScript.cs");
    }

    private static void TestScript(string filename)
    {
        var generator = new AsmGenerator();
        var content = File.ReadAllText($@"..\..\..\TestFiles\{filename}");

        GeneratorTestUtilities.TestGenerator(generator, content);
    }
}
