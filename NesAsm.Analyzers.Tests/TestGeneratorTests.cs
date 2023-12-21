using ApprovalTests;
using ApprovalTests.Reporters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NesAsm.Analyzers.Tests;

public class TestGeneratorTests
{
    [Fact]
    [UseReporter(typeof(VisualStudioReporter))]
    public void TestGenerator()
    {
        var generator = new TestGenerator();
        var source = @"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
";

        GeneratorTestUtilities.TestGenerator(generator, source);
    }

    [Fact]
    [UseReporter(typeof(VisualStudioReporter))]
    public void TestIncrementalGenerator()
    {
        var generator = new Generator();
        var source = (@"
using NesAsm.Emulator;
namespace MyCode;
public class Program : NesAsm.Emulator.ScriptBase
{
    public Program(NESEmulator emulator) : base(emulator) {}
}
");

        GeneratorTestUtilities.TestGenerator(generator, source);
    }
}