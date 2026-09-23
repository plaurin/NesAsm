namespace NesAsm.Emulator.AddressModes;

public class IndirectY(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    // TODO Extra cycle
    public override void SetValue(byte value) => cpu.Memory.Write(GetAddress(), value);

    public override ushort GetAddress()
    {
        var lo = cpu.Memory.Read(cpu.PeekByteArgument());
        var hi = cpu.Memory.Read((byte)(cpu.PeekByteArgument() + 1));
        var address = (ushort)(hi * 256 + lo + cpu.Y);
        return address;
    } 
}
