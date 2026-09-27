namespace NesAsm.Emulator.AddressModes;

public class Indirect(CPU cpu) : AddressMode
{
    public override int Bytes => 2;
    public override byte GetValue() => throw new NotImplementedException();
    // TODO Extra cycle
    public override void SetValue(byte value) => throw new NotImplementedException();
    public override ushort GetAddress()
    {
        var lo = cpu.Memory.Read(cpu.PeekByteArgument());
        var hi = cpu.Memory.Read((byte)(cpu.PeekByteArgument() + 1));
        var address = (ushort)(hi * 256 + lo);
        return address;
    }
}
