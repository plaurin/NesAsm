namespace NesAsm.Emulator;

public class NesMemory
{
    public byte[] _ram = new byte[0x800];

    public byte Read(ushort address)
    {
        if (address < 0x800) return _ram[address];

        return 0;
    }

    public void Write(ushort address, byte value)
    {
        if (address < 0x800) _ram[address] = value;
    }
}
