using NesAsm.Emulator.Mappers;

namespace NesAsm.Emulator;

public abstract class Cart
{
    protected readonly byte[] _rom = [];
    protected readonly byte[] _prgRam = [];
    protected readonly byte[] _chrRam = [];

    public string RomPath { get; init; }
    public byte NbPrgBanks { get; init; }
    public ushort PrgRomOffset { get; init; }
    public byte NbChrBanks { get; init; }
    public ReadOnlySpan<byte> PrgRom => _rom.AsSpan(start: 16, length: PrgSize);
    public ReadOnlySpan<byte> ChrRom => _rom.AsSpan(start: 16 + PrgSize, length: ChrSize);
    public Span<byte> ChrRam => _chrRam;
    public ushort NmiAddress { get; init; }
    public ushort ResetAddress { get; init; }
    public ushort IrqAddress { get; init; }
    public bool IsVerticalMirroring { get; init; }
    public bool IsHorizontalMirroring => !IsVerticalMirroring;
    public bool HasBattery { get; init; }
    public bool HasTrainer { get; init; }
    public bool IsFourScreen { get; init; }
    public int MapperId { get; init; }
    public bool IsValid { get; private set; }

    public int PrgSize => NbPrgBanks * 16 * 1024;
    public int ChrSize => NbChrBanks * 8 * 1024;

    public static Cart Create(string romPath)
    {
        var _rom = File.ReadAllBytes(romPath);
        var flag7 = _rom[7];
        var flag6 = _rom[6];
        var MapperId = (flag7 & 0xF0) | (flag6 >> 4);

        return new CartMapper0(romPath);
    }

    protected Cart(string romPath)
    {
        RomPath = romPath;
        _rom = File.ReadAllBytes(romPath);

        if (!(_rom[0] == 0x4E && _rom[1] == 0x45 && _rom[2] == 0x53 && _rom[3] == 0x1A))
        {
            Console.WriteLine("NES ROM not detected!");
            return;
        }

        Console.WriteLine("NES ROM detected.");

        NbPrgBanks = _rom[4];
        PrgRomOffset = (ushort)(NbPrgBanks == 1 ? 0xC000 : 0x8000);

        NbChrBanks = _rom[5];

        _prgRam = new byte[0x2000];
        _chrRam = new byte[0x2000];

        var flag6 = _rom[6];
        var flag7 = _rom[7];

        IsVerticalMirroring = (flag6 & 0x01) != 0;
        HasBattery = (flag6 & 0x02) != 0;
        HasTrainer = (flag6 & 0x04) != 0;
        IsFourScreen = (flag6 & 0x08) != 0;
        MapperId = (flag7 & 0xF0) | (flag6  >> 4);

        Console.WriteLine($"Mirroring: {(IsVerticalMirroring ? "Vertical" : "Horizontal")}");
        Console.WriteLine($"Battery Backed: {(HasBattery ? "Yes" : "No")}");
        Console.WriteLine($"Trainer: {(HasTrainer ? "Yes" : "No")}");
        Console.WriteLine($"Four Screen: {(IsFourScreen ? "Yes" : "No")}");
        Console.WriteLine($"Mapper (Low): {MapperId}");

        NmiAddress = (ushort)(PrgRom[^6] + (PrgRom[^5] << 8));
        ResetAddress = (ushort)(PrgRom[^4] + (PrgRom[^3] << 8));
        IrqAddress = (ushort)(PrgRom[^2] + (PrgRom[^1] << 8));

        Console.WriteLine($"NMI: ${NmiAddress:X4}");
        Console.WriteLine($"Reset: ${ResetAddress:X4}");
        Console.WriteLine($"IRQ: ${IrqAddress:X4}");

        if (MapperId != 0)
            IsValid = false;
        else
            IsValid = true;
    }

    public abstract byte ReadPrgMemory(ushort address);

    public abstract byte ReadByteArgument(ushort baseAddress);

    public abstract ushort ReadWordArgument(ushort baseAddress);

    public abstract byte ReadChrMemory(ushort address);

    public abstract void WriteChrMemory(ushort address, byte value);
}