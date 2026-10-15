namespace NesAsm.Emulator.AddressModes;

public class AbsoluteX(CPU cpu) : AddressMode
{
    public override int Bytes => 2;
    public override int GetExtraCycle(ushort cpuAddress) => GetExtraCycle(cpu.PeekWordArgument(), GetAddress());
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    public override void SetValue(byte value) => cpu.Memory.Write(GetAddress(), value);
    public override ushort GetAddress() => (ushort)(cpu.PeekWordArgument() + cpu.X);
}
