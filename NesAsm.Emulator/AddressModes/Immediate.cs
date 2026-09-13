namespace NesAsm.Emulator.AddressModes;

public class Immediate(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.NextByte();
    public override void SetValue(byte value) => throw new NotSupportedException();
}
