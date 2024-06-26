﻿using ApprovalTests.Reporters;

namespace NesAsm.Analyzers.Tests.AsmTests;

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
    public void TestCharScript()
    {
        TestScript("CharScript.cs");
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
        TestScript("CallingOtherScript.cs", "MultiProcScript.cs", @"..\UpFolderScript.cs", @"SubFolder\SubFolderScript.cs");
    }

    [Fact]
    public void TestDataScript()
    {
        TestScript("DataScript.cs");
    }

    [Fact]
    public void TestDataGeneratorScript()
    {
        TestScript("DataGeneratorScript.cs");
    }

    [Fact]
    public void TestAllInstructionsScript()
    {
        TestScript("AllInstructionsScript.cs");
    }

    [Fact]
    public void TestStartupScript()
    {
        TestScript("StartupScript.cs");
    }

    [Fact]
    public void TestIterationStatementsScript()
    {
        TestScript("IterationStatementsScript.cs");
    }

    [Fact]
    public void TestSelectionStatementsScript()
    {
        TestScript("SelectionStatementsScript.cs");
    }

    [Fact]
    public void TestMacroScript()
    {
        TestScript("MacroScript.cs");
    }

    [Fact]
    public void TestCallingOtherMacroScript()
    {
        TestScript("CallingOtherMacroScript.cs", "MacroScript.cs");
    }

    private static void TestScript(params string[] filenames)
    {
        var generator = new AsmGenerator();

        var sourcePaths = filenames.Select(filename => $@"..\..\..\AsmTests\TestFiles\{filename}").ToArray();

        GeneratorTestUtilities.TestGenerator(sourcePaths, generator);
    }
}
