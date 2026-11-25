namespace NesAsm.Emulator.Mappers;

public class CartMapper0 : Cart
{
    public CartMapper0(string romPath) 
        : base(romPath)
    {
    }

    public override byte ReadPrgMemory(ushort address)
    {
        return PrgRom[address - PrgRomOffset];
    }

    public override byte ReadByteArgument(ushort baseAddress)
    {
        return PrgRom[baseAddress + 1 - PrgRomOffset];
    }

    public override ushort ReadWordArgument(ushort baseAddress)
    {
        return (ushort)(PrgRom[baseAddress + 1 - PrgRomOffset] | (PrgRom[baseAddress + 2 - PrgRomOffset] << 8));
    }

    public override byte ReadChrMemory(ushort address)
    {
        if (NbChrBanks != 0)
            return ChrRom[address];
        else
            return _chrRam[address];
    }

    public override void WriteChrMemory(ushort address, byte value)
    {
        if (NbChrBanks == 0)
            _chrRam[address] = value;
    }
}