namespace NesAsm.Emulator.AddressModes;

public class ZeroPage(CPU cpu) : AddressMode
{
    public override byte GetValue() => cpu.Memory.Read(cpu.NextByte());
    public override void SetValue(byte value) => cpu.Memory.Write(cpu.NextByte(), value);
}
