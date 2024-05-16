using ApprovalTests.Reporters;
using NesAsm.Analyzers;
using NesAsm.Analyzers.Tests;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace NesAsm.Example.Tests;

[UseReporter(typeof(VisualStudioReporter))]
public class GameTests
{
    private readonly ITestOutputHelper _output;

    public GameTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestHiloWorldGame()
    {
        TestTranspileGame(@"HiloWorld\HiloWorld.cs");

        var gameInfo = new GameInfo { GameFolder = "HiloWorld", AsmFileToCompile = "HiloWorld.s", NesOutputFile = "hiloworld.nes" };
        TestCompileGame(gameInfo);
    }

    [Fact]
    public void TestGame1()
    {
        TestTranspileGame(@"Game1\Game1.cs", "Controller.cs");

        var gameInfo = new GameInfo { GameFolder = "Game1", AsmFileToCompile = "game1.s", NesOutputFile = "game1.nes" };
        TestCompileGame(gameInfo);
    }

    [Fact]
    public void TestGame1C()
    {
        TestTranspileGame(@"Game1\Game1C.cs", @"Game1\Game1Char.cs", "Controller.cs", "PPU.cs");

        var gameInfo = new GameInfo { GameFolder = "Game1", AsmFileToCompile = "game1c.s", NesOutputFile = "game1c.nes" };
        TestCompileGame(gameInfo);
    }

    [Fact]
    public void TestGame2C()
    {
        TestTranspileGame(@"Game2\Game2C.cs", @"Game2\Game2Char.cs", "Controller.cs", "PPU.cs");

        var gameInfo = new GameInfo { GameFolder = "Game2", AsmFileToCompile = "game2c.s", NesOutputFile = "game2c.nes" };
        TestCompileGame(gameInfo);
    }

    private static void TestTranspileGame(params string[] filenames)
    {
        var asmGenerator = new AsmGenerator();
        var charGenerator = new CharGenerator();

        var sourcePaths = filenames.Select(filename => Path.Combine(@"..\..\..\..\NesAsm.Example", filename)).ToArray();

        GeneratorTestUtilities.TestGenerator(sourcePaths, asmGenerator, charGenerator);
    }

    private void TestCompileGame(GameInfo gameInfo)
    {
        var psi = new ProcessStartInfo()
        {
            WorkingDirectory = Path.GetFullPath($@"..\..\..\..\NesAsm.Example\{gameInfo.GameFolder}"),
            FileName = @"c:\cc65\bin\cl65",
            Arguments = $"--verbose --target nes -o {gameInfo.NesOutputFile} {gameInfo.AsmFileToCompile}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var process = Process.Start(psi);

        process!.OutputDataReceived += (s, e) => _output.WriteLine(e.Data ?? "");
        process.ErrorDataReceived += (s, e) => _output.WriteLine(e.Data != null ? $"ERR : {e.Data}" : "");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process!.WaitForExit();

        Assert.Equal(0, process.ExitCode);
    }

    private class GameInfo
    {
        public string GameFolder { get; set; } = null!;
        public string AsmFileToCompile { get; set; } = null!;
        public string NesOutputFile { get; set; } = null!;
    }
}
