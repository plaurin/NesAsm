namespace NesAsm.Emulator.AddressModes;

public class IndirectY(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override int GetExtraCycle(ushort cpuAddress) => GetExtraCycle(GetBaseAddress(), GetAddress());
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    public override void SetValue(byte value) => cpu.Memory.Write(GetAddress(), value);

    public ushort GetBaseAddress()
    {
        var lo = cpu.Memory.Read(cpu.PeekByteArgument());
        var hi = cpu.Memory.Read((byte)(cpu.PeekByteArgument() + 1));
        var address = (ushort)(hi * 256 + lo);
        return address;
    }

    public override ushort GetAddress()
    {
        var address = (ushort)(GetBaseAddress() + cpu.Y);
        return address;
    } 
}
