namespace NesAsm.Emulator.AddressModes;

public class Immediate(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => cpu.PeekByteArgument();
    public override void SetValue(byte value) => throw new NotSupportedException();
    public override ushort GetAddress() => 0;
}
