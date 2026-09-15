namespace NesAsm.Emulator.AddressModes;

public class ZeroPageY(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => cpu.Memory.Read((ushort)(GetAddress() + cpu.Y));
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(GetAddress() + cpu.Y), value);
    public override ushort GetAddress() => cpu.PeekByteArgument();
}
