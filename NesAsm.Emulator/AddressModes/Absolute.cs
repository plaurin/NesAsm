namespace NesAsm.Emulator.AddressModes;

public class Absolute(CPU cpu) : AddressMode
{
    public override int Bytes => 2;
    public override int GetExtraCycle(ushort cpuAddress) => 0;
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    public override void SetValue(byte value) => cpu.Memory.Write(GetAddress(), value);
    public override ushort GetAddress() => cpu.PeekWordArgument();
}
