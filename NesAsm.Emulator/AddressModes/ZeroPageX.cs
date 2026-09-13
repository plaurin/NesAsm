namespace NesAsm.Emulator.AddressModes;

public class ZeroPageX(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.Memory.Read((ushort)(cpu.NextByte() + cpu.X));
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(cpu.NextByte() + cpu.X), value);
}
