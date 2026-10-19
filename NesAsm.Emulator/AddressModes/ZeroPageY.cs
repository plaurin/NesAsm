namespace NesAsm.Emulator.AddressModes;

public class ZeroPageY(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override int GetExtraCycle(ushort cpuAddress) => 0;
    public override byte GetValue() => cpu.Memory.Read((byte)(GetAddress() + cpu.Y));
    public override void SetValue(byte value) => cpu.Memory.Write((byte)(GetAddress() + cpu.Y), value);
    public override ushort GetAddress() => cpu.PeekByteArgument();
}
