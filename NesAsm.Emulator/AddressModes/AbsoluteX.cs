namespace NesAsm.Emulator.AddressModes;

public class AbsoluteX(CPU cpu) : AddressMode
{
    public override int Bytes => 2;
    public override byte GetValue() => cpu.Memory.Read((ushort)(GetAddress() + cpu.X));
    // TODO Extra cycle
    public override void SetValue(byte value) => cpu.Memory.Write((ushort)(GetAddress() + cpu.X), value);
    public override ushort GetAddress() => cpu.PeekWordArgument();
}
