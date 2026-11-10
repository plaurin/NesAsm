namespace NesAsm.Emulator.Tests;

public class CartTests
{
    public static TheoryData<string, string> Mapper0Roms()
    {
        var data = new TheoryData<string, string>();

        foreach (var file in File.ReadLines(Path.Combine(GetRomFolder(), "mapper0_nrom.txt")))
        {
            data.Add(Path.GetFileName(file), Path.GetDirectoryName(file)!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Mapper0Roms))]
    public void TestLoadMapper0Cart(string romFile, string folder)
    {
        var cart = new Cart(Path.Combine(folder, romFile));

        if (cart.MapperId == 0)
        {
            Assert.True(cart.IsValid);
        }
    }

    [Theory]
    [MemberData(nameof(Mapper0Roms))]
    public void TestRunMapper0Cart(string romFile, string folder)
    {
        var cart = new Cart(Path.Combine(folder, romFile));

        // Setup
        var _memory = new NesMemory(cart);
        var _ppu = new PPUInstance(_memory);
        _memory.SetPPU(_ppu);
        var _cpu = new CPU(_memory);
        _memory.SetCPU(_cpu);

        // Run
        _cpu.Init();

        // Loop
        while (_ppu.Frame < 3000)
        {
            var ins = _cpu.RunNextInstruction();
            _ppu.RunToCycle(_cpu.Cycles);

            if (_ppu.VBlankStart)
            {
                _ppu.VBlankStart = false;
            }

            if (_ppu.NmiRequested)
            {
                _ppu.NmiRequested = false;
                _cpu.RunNmi();
            }
        }
    }

    [Theory]
    [MemberData(nameof(Mapper0Roms))]
    public async Task TestRunnerMapper0Cart(string romFile, string folder)
    {
        var cart = new Cart(Path.Combine(folder, romFile));
        var runner = new NesRunner(cart);

        var cts = new CancellationTokenSource();
        await runner.RunGame(cts.Token, () =>
        {
            if (runner.Frame >= 300)
                cts.Cancel();
        });
    }

    [Fact]
    public void ScanMappers()
    {
        var rootFolder = File.ReadAllText(Path.Combine(GetRomFolder(), "cart_test_path.txt"));

        var files = Directory.GetFiles(rootFolder, "*.nes", SearchOption.AllDirectories);

        var mapper0 = new List<string>(); // NROM
        var mapper1 = new List<string>(); // MMC1
        var mapper2 = new List<string>(); // UxROM
        var mapper3 = new List<string>(); // CNROM
        var mapper4 = new List<string>(); // MMC3
        var mapper5 = new List<string>(); // MMC5
        var mapper7 = new List<string>(); // AxROM
        var other = new List<string>(); // others

        foreach (var file in files)
        {
            var cart = new Cart(file);
            switch (cart.MapperId)
            {
                case 0:
                    mapper0.Add(file);
                    break;
                case 1:
                    mapper1.Add(file);
                    break;
                case 2:
                    mapper2.Add(file);
                    break;
                case 3:
                    mapper3.Add(file);
                    break;
                case 4:
                    mapper4.Add(file);
                    break;
                case 5:
                    mapper5.Add(file);
                    break;
                case 7:
                    mapper7.Add(file);
                    break;
                default:
                    other.Add($"{cart.MapperId,3} {file}");
                    break;
            }
        }

        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper0_nrom.txt"), mapper0);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper1_mmc1.txt"), mapper1);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper2_uxrom.txt"), mapper2);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper3_cnrom.txt"), mapper3);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper4_mmc3.txt"), mapper4);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper5_mmc5.txt"), mapper5);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper7_axrom.txt"), mapper7);
        File.WriteAllLines(Path.Combine(GetRomFolder(), "mapper_other.txt"), other);
    }

    private static string GetRomFolder()
    {
        const string projectFolder = "NesAsm.Emulator.Tests";
        var cur = Directory.GetCurrentDirectory();
        var ind = cur.IndexOf(projectFolder) + projectFolder.Length;
        return Path.Combine(cur[..ind], "rom");
    }
}
