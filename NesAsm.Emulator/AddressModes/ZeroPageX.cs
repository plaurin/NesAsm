namespace NesAsm.Emulator.AddressModes;

public class ZeroPageX(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => cpu.Memory.Read((ushort)(GetAddress() + cpu.X));
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(GetAddress() + cpu.X), value);
    public override ushort GetAddress() => cpu.PeekByteArgument();
}
