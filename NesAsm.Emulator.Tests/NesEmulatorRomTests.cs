using Xunit.Abstractions;

namespace NesAsm.Emulator.Tests;

public class NesEmulatorRomTests
{
    private readonly NesMemory _memory;
    private readonly PPUInstance _ppu;
    private readonly CPU _cpu;
    private readonly string[] _logs;
    private readonly ITestOutputHelper _output;

    public NesEmulatorRomTests(ITestOutputHelper output)
    {
        _output = output;

        var log = @"C:\Users\pasca\Dev\GitHub\NesAsm\NesAsm.Emulator.Tests\rom\nestest.log";
        _logs = File.ReadAllLines(log);

        var path = @"C:\Users\pasca\Dev\GitHub\NesAsm\NesAsm.Emulator.Tests\rom\nestest.nes";

        var cart = new Cart(path);
        _memory = new NesMemory(cart);
        _ppu = new PPUInstance(_memory);
        _memory.SetPPU(_ppu);
        _cpu = new CPU(_memory);
        _memory.SetCPU(_cpu);

        _cpu.Init();
    }

    [Fact]
    public void Test()
    {
        _cpu.SetPC(0xC000);
        _ppu.RunToCycle(_cpu.Cycles);

        int logIndex = 0;
        var previousLog = GetCurrentLog();

        while (_ppu.Frame <= 11250)
        {
            var actualLog = GetCurrentLog();
            var expectedLog = _logs[logIndex++];
            AssertLogs(previousLog, actualLog, expectedLog);
            previousLog = expectedLog;

            try
            {
                if (_cpu.PC == 0xD922) { }
                _cpu.RunNextInstruction();
                _ppu.RunToCycle(_cpu.Cycles);
            }
            catch (Exception)
            {
                _output.WriteLine($"Failed at: {_cpu.PC:X4}");
                throw;
            }

            if (_ppu.NmiRequested)
            {
                _ppu.NmiRequested = false;

                _cpu.RunNmi();
            }
        }
    }

    private string GetCurrentLog()
    {
        var addr = _cpu.PC;
        var ins = _cpu.GetInstructionAt(addr);
        var bytes = ins.AddressMode.Bytes switch
        {
            0 => $"{ins.Opcode:X2}",
            1 => $"{ins.Opcode:X2} {_memory.Peek((ushort)(addr + 1)):X2}",
            2 => $"{ins.Opcode:X2} {_memory.Peek((ushort)(addr + 1)):X2} {_memory.Peek((ushort)(addr + 2)):X2}",
            _ => throw new NotImplementedException()
        };

        var states = $"A:{_cpu.A:X2} X:{_cpu.X:X2} Y:{_cpu.Y:X2} P:{_cpu.P:X2} SP:{_cpu.SP:X2} PPU:{_ppu.Scanline,3},{_ppu.ScanlineCycle,3} CYC:{_cpu.Cycles}";

        var actualLog = $"{addr:X4}  {bytes,-9} {ins.Mnemonic} {states}";

        return actualLog;
    }

    private void AssertLogs(string previousLog, string actualLog, string expectedLog)
    {
        expectedLog = string.Concat(expectedLog.AsSpan(0, 20), expectedLog.AsSpan(48));
        if (actualLog != expectedLog)
        {
            _output.WriteLine($"Previous State: {previousLog.AsSpan(48)}");
            _output.WriteLine($"Instruction:    {previousLog.AsSpan(0, 48)}");
            _output.WriteLine($"Expected State: {expectedLog[20..]}");
            _output.WriteLine($"Actual State:   {actualLog[20..]}");
        }

        Assert.Equal(expectedLog, actualLog);
    }
}
