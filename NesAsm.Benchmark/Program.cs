using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NesAsm.Emulator;

var summary = BenchmarkRunner.Run<TestBenchmark>();

[ShortRunJob]
public class TestBenchmark
{

    public TestBenchmark()
    {
    }

    [Benchmark]
    public void PPUDrawAll()
    {
        for (var i = 0; i < 60; i++) PPU.DrawScreen();
    }
}