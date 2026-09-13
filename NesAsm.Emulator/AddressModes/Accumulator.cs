namespace NesAsm.Emulator.AddressModes;

public class Accumulator(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.A;
    public override void SetValue(byte value) => throw new NotSupportedException();
}
