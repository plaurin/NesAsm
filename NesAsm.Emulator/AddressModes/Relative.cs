namespace NesAsm.Emulator.AddressModes;

public class Relative(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => cpu.Memory.Read(GetAddress());
    // TODO Extra cycle

    public override void SetValue(byte value) => throw new NotImplementedException();
    public override ushort GetAddress() => (ushort)(cpu.PC + (sbyte)cpu.PeekByteArgument());
}
