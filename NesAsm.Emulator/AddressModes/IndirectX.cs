namespace NesAsm.Emulator.AddressModes;

public class IndirectX(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override int GetExtraCycle(ushort cpuAddress) => 0;
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    public override void SetValue(byte value) => cpu.Memory.Write(GetAddress(), value);

    public override ushort GetAddress()
    {
        var lo = cpu.Memory.Read((byte)(cpu.PeekByteArgument() + cpu.X));
        var hi = cpu.Memory.Read((byte)(cpu.PeekByteArgument() + cpu.X + 1));
        var address = (ushort)(hi * 256 + lo);
        return address;
    }
}
