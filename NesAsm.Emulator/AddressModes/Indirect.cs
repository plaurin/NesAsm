namespace NesAsm.Emulator.AddressModes;

public class Indirect(CPU cpu) : AddressMode
{
    public override int Bytes => 2;
    public override int GetExtraCycle(ushort cpuAddress) => 0;
    public override byte GetValue() => throw new NotImplementedException();
    public override void SetValue(byte value) => throw new NotImplementedException();
    public override ushort GetAddress()
    {
        var arg = cpu.PeekWordArgument();
        var lo = cpu.Memory.Read(arg);
        var hi = (arg & 0x00FF) == 0x00FF ? cpu.Memory.Read((ushort)(arg & 0xFF00)) : cpu.Memory.Read((ushort)(arg + 1));
        var address = (ushort)(hi * 256 + lo);
        return address;
    }
}
