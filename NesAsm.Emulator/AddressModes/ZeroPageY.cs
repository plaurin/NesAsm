namespace NesAsm.Emulator.AddressModes;

public class ZeroPageY(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.Memory.Read((ushort)(cpu.NextByte() + cpu.Y));
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(cpu.NextByte() + cpu.Y), value);
}
