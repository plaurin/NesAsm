namespace NesAsm.Emulator;

public class NesMemory
{
    public byte[] _ram = new byte[0x800];
    private CPU _cpu = null!;

    public NesMemory(Cart cart, PPUInstance ppu)
    {
        Cart = cart;
        Ppu = ppu;
    }

    public Cart Cart { get; init; }
    public PPUInstance Ppu { get; init; }

    public Dictionary<ushort, HashSet<ushort>> Reads { get; } = [];

    public Dictionary<ushort, HashSet<ushort>> Writes { get; } = [];

    public void SetCPU(CPU cpu)
    {
        _cpu = cpu;
    }

    public byte Read(ushort address)
    {
        var value = Peek(address);

        RecordAccess(address, _cpu.PC, Reads);

        return value;
    }

    public byte Peek(ushort address)
    {
        if (address < 0x800)
            return _ram[address];
        else if (address >= 0x6000 && address <= 0xFFFF)
            return Cart.ReadMemory(address);
        else if (address >= 0x2000 && address <= 0x3FFF)
            return Ppu.ReadRegister(address);

        return 0;
    }

    public byte PeekByteArgument(ushort baseAddress)
    {
        if (baseAddress >= 0x6000 && baseAddress <= 0xFFFF)
            return Cart.ReadByteArgument(baseAddress);

        return 0; // should throw
    }

    public ushort PeekWordArgument(ushort baseAddress)
    {
        if (baseAddress >= 0x6000 && baseAddress <= 0xFFFF)
            return Cart.ReadWordArgument(baseAddress);

        return 0; // should throw
    }

    public void Write(ushort address, byte value)
    {
        if (address < 0x800)
            _ram[address] = value;
        else if (address >= 0x2000 && address <= 0x3FFF)
            Ppu.WriteRegister(address, value);
        // TODO PPU and others

        RecordAccess(address, _cpu.PC, Writes);
    }

    private static void RecordAccess(ushort address, ushort pc, Dictionary<ushort, HashSet<ushort>> dict)
    {
        if (!dict.TryGetValue(pc, out var hashset))
        {
            hashset = [];
            dict.TryAdd(pc, hashset);
        }
        hashset.Add(address);
    }
}
