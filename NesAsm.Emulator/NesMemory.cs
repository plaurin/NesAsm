using System.Net;

namespace NesAsm.Emulator;

public class NesMemory
{
    public byte[] _ram = new byte[0x800];

    public NesMemory(Cart cart, PPUInstance ppu)
    {
        Cart = cart;
        Ppu = ppu;
    }

    public Cart Cart { get; init; }
    public PPUInstance Ppu { get; private set; }

    public byte Read(ushort address)
    {
        if (address < 0x800)
            return _ram[address];
        else if (address >= 0x6000 && address <= 0xFFFF)
            return Cart.ReadMemory(address);
        else if (address >= 0x2000 && address <= 0x3FFF)
            return Ppu.ReadRegister(address);

        return 0;
    }

    public byte ReadByteArgument(ushort baseAddress)
    {
        if (baseAddress >= 0x6000 && baseAddress <= 0xFFFF)
            return Cart.ReadByteArgument(baseAddress);

        return 0; // should throw
    }

    public ushort ReadWordArgument(ushort baseAddress)
    {
        if (baseAddress >= 0x6000 && baseAddress <= 0xFFFF)
            return Cart.ReadWordArgument(baseAddress);

        return 0; // should throw
    }

    public void Write(ushort address, byte value)
    {
        if (address < 0x800) _ram[address] = value;
        // TODO PPU and others
    }
}
