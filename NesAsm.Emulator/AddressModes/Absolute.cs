namespace NesAsm.Emulator.AddressModes;

public class Absolute(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.Memory.Read(cpu.NextWord());
    // TODO Extra cycle
    public override void SetValue(byte value) => cpu.Memory.Write(cpu.NextWord(), value);
}
