namespace NesAsm.Emulator.AddressModes;

public class AbsoluteY(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.Memory.Read((ushort)(cpu.NextWord() + cpu.Y));
    // TODO Extra cycle
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(cpu.NextWord() + cpu.Y), value);
}
