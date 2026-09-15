using System.Net;

namespace NesAsm.Emulator;

public class NesMemory
{
    public byte[] _ram = new byte[0x800];

    public NesMemory(Cart cart)
    {
        Cart = cart;
    }

    public Cart Cart { get; init; }

    public byte Read(ushort address)
    {
        if (address < 0x800)
            return _ram[address];
        else if (address >= 0x6000 && address <= 0xFFFF)
            return Cart.ReadMemory(address);

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
