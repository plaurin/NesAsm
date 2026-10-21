namespace NesAsm.Emulator;

public class NesMemory
{
    public byte[] _ram = new byte[0x800];
    private CPU _cpu = null!;
    private PPUInstance _ppu = null!;

    public NesMemory(Cart cart)
    {
        Cart = cart;
    }

    public Cart Cart { get; init; }

    public Dictionary<ushort, HashSet<ushort>> Reads { get; } = [];

    public Dictionary<ushort, HashSet<ushort>> Writes { get; } = [];

    public void SetCPU(CPU cpu) => _cpu = cpu;
    public void SetPPU(PPUInstance ppu) => _ppu = ppu;

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
        else if (address == 0x4016) // Input player 1
            return InputManager.ReadBuffer();
        else if (address == 0x4017) // Input player 2
            return InputManager.ReadBufferPlayer2();
        else if (address >= 0x6000 && address <= 0xFFFF)
            return Cart.ReadMemory(address);
        else if (address >= 0x2000 && address <= 0x3FFF)
            return _ppu.ReadRegister(address);

        return 0;
    }

    public byte PeekByteArgument(ushort baseAddress)
    {
        if (baseAddress >= 0x6000 && baseAddress <= 0xFFFF)
            return Cart.ReadByteArgument(baseAddress);
        else if (baseAddress >= 0x0 && baseAddress < 0x800)
            return _ram[baseAddress + 1];

        return 0; // should throw
    }

    public ushort PeekWordArgument(ushort baseAddress)
    {
        if (baseAddress >= 0x6000 && baseAddress <= 0xFFFF)
            return Cart.ReadWordArgument(baseAddress);

        return 0; // should throw
    }

    public void Write(ushort address, byte value, bool? shouldRecord = true)
    {
        if (address < 0x800)
            _ram[address] = value;
        else if (address >= 0x2000 && address <= 0x3FFF)
            _ppu.WriteRegister(address, value);
        else if (address == 0x4014) // OAMDMA
            _ppu.WriteOAMDMA(value);
        else if (address >= 0x4000 && address <= 0x4013 || address == 0x4015) { } //APU
        else if (address == 0x4016) // Input player 1
        {
            if ((value & 1) != 0) InputManager.TransfertStateToBuffer();
        }
        else if (address == 0x4017) // Input player 2
        {
            //if ((value & 1) != 0) InputManager.TransfertStateToBuffer();
        }
        else
        {
            throw new NotImplementedException();
        }
        // TODO PPU and others

        if (shouldRecord ?? true)
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
