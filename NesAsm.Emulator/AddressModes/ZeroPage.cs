namespace NesAsm.Emulator.AddressModes;

public class ZeroPage(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    public override void SetValue(byte value) => cpu.Memory.Write(GetAddress(), value);
    public override ushort GetAddress() => cpu.PeekByteArgument();
}
