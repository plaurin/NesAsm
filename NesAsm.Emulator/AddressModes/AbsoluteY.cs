namespace NesAsm.Emulator.AddressModes;

public class AbsoluteY(CPU cpu) : AddressMode
{
    public override int Bytes => 2;
    public override byte GetValue() => cpu.Memory.Read((ushort)(GetAddress() + cpu.Y));
    // TODO Extra cycle
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(GetAddress() + cpu.Y), value);
    public override ushort GetAddress() => cpu.PeekWordArgument();
}
