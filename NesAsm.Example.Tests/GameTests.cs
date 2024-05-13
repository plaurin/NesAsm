using ApprovalTests.Reporters;
using NesAsm.Analyzers;
using NesAsm.Analyzers.Tests;
using Xunit;

namespace NesAsm.Example.Tests;

[UseReporter(typeof(VisualStudioReporter))]
public class GameTests
{
    [Fact]
    public void TestHiloWorldGame()
    {
        TestGame(@"HiloWorld\HiloWorld.cs");
    }

    [Fact]
    public void TestGame1()
    {
        TestGame(@"Game1\Game1C.cs", @"Game1\Game1Char.cs", "Controller.cs", "PPU.cs");
    }

    [Fact]
    public void TestGame2()
    {
        TestGame(@"Game2\Game2C.cs", @"Game2\Game2Char.cs", "Controller.cs", "PPU.cs");
    }

    private static void TestGame(params string[] filenames)
    {
        var asmGenerator = new AsmGenerator();
        var charGenerator = new CharGenerator();

        var sourcePaths = filenames.Select(filename => Path.Combine(@"..\..\..\..\NesAsm.Example", filename)).ToArray();

        GeneratorTestUtilities.TestGenerator(sourcePaths, asmGenerator, charGenerator);
    }
}
