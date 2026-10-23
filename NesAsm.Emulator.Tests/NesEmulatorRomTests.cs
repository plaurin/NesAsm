using NesAsm.Emulator.Instructions;
using Xunit.Abstractions;

namespace NesAsm.Emulator.Tests;

public class NesEmulatorRomTests
{
    private NesMemory _memory;
    private PPUInstance _ppu;
    private CPU _cpu;
    private readonly ITestOutputHelper _output;

    public NesEmulatorRomTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void NesTestOfficialOpCodes()
    {
        // https://github.com/christopherpow/nes-test-roms/blob/master/blargg_ppu_tests_2005.09.15b/readme.txt
        InitCart(@"nestest.nes", 0xC000);

        var logFile = Path.Combine(GetRomFolder(), @"nestest.log");
        var logs = File.ReadAllLines(logFile);

        int logIndex = 0;
        var previousLog = GetCurrentLog();

        void BeforeNextInstruction(Instruction ins)
        {
            var actualLog = GetCurrentLog();
            var expectedLog = logs[logIndex++];
            AssertLogs(previousLog, actualLog, expectedLog);
            previousLog = expectedLog;

            if (_cpu.PC == 0xCFFE) { }
        }

        RunProgram(0xC6BD, BeforeNextInstruction);
    }

    [Fact]
    public void PaletteRamTest()
    {
        // https://github.com/christopherpow/nes-test-roms/blob/master/blargg_ppu_tests_2005.09.15b/readme.txt
        InitCart("palette_ram.nes");

        void BeforeNextInstruction(Instruction ins)
        {
            if (_cpu.PC == 0xE0E2) { }
        }

        RunProgram(0xE412, BeforeNextInstruction);

        Assert.Equal(1, _memory.Peek(0xF0));
    }

    [Fact]
    public void SpriteRamTest()
    {
        // https://github.com/christopherpow/nes-test-roms/blob/master/blargg_ppu_tests_2005.09.15b/readme.txt
        InitCart("sprite_ram.nes");

        void BeforeNextInstruction(Instruction ins)
        {
            if (_cpu.PC == 0xE045) { }
        }

        RunProgram(0xE467, BeforeNextInstruction);

        Assert.Equal(1, _memory.Peek(0xF0));
    }

    [Fact]
    public void VramAccessTest()
    {
        // https://github.com/christopherpow/nes-test-roms/blob/master/blargg_ppu_tests_2005.09.15b/readme.txt
        InitCart("vram_access.nes");

        void BeforeNextInstruction(Instruction ins)
        {
            if (_cpu.PC == 0xE045) { }
        }

        RunProgram(0xE48D, BeforeNextInstruction);

        Assert.Equal(1, _memory.Peek(0xF0));
    }

    private static string GetRomFolder()
    {
        const string projectFolder = "NesAsm.Emulator.Tests";
        var cur = Directory.GetCurrentDirectory();
        var ind = cur.IndexOf(projectFolder) + projectFolder.Length;
        return Path.Combine(cur[..ind], "rom");
    }

    private void InitCart(string romFile, ushort? startAddress = null)
    {
        var path = Path.Combine(GetRomFolder(), romFile);

        var cart = new Cart(path);
        _memory = new NesMemory(cart);
        _ppu = new PPUInstance(_memory);
        _memory.SetPPU(_ppu);
        _cpu = new CPU(_memory);
        _memory.SetCPU(_cpu);

        _cpu.Init();

        if (startAddress.HasValue)
            _cpu.SetPC(startAddress.Value);

        _ppu.RunToCycle(_cpu.Cycles);
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

        var star = ins.IsOfficialOpCode ? " " : "*";

        var states = $"A:{_cpu.A:X2} X:{_cpu.X:X2} Y:{_cpu.Y:X2} P:{_cpu.P:X2} SP:{_cpu.SP:X2} PPU:{_ppu.Scanline,3},{_ppu.ScanlineCycle,3} CYC:{_cpu.Cycles}";

        var actualLog = $"{addr:X4}  {bytes,-9}{star}{ins.Mnemonic} {states}";

        return actualLog;
    }

    private void AssertLogs(string previousLog, string actualLog, string expectedLog)
    {
        expectedLog = string.Concat(expectedLog.AsSpan(0, 20), expectedLog.AsSpan(48));
        if (actualLog != expectedLog)
        {
            _output.WriteLine($"Previous State:      {previousLog.AsSpan(48)}");
            _output.WriteLine($"Instruction:    {previousLog.AsSpan(0, 48)}");
            _output.WriteLine($"Expected State: {expectedLog.AsSpan(0, 5)}{expectedLog[20..]}");
            _output.WriteLine($"Actual State:   {actualLog.AsSpan(0, 5)}{actualLog[20..]}");
        }

        Assert.Equal(expectedLog, actualLog);
    }

    private void RunProgram(ushort endAddress, Action<Instruction>? beforeNextInstruction = null)
    {
        while (_cpu.PC != endAddress)
        {
            var ins = _cpu.GetInstructionAt(_cpu.PC);
            beforeNextInstruction?.Invoke(ins);

            try
            {
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
}
