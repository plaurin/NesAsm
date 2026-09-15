namespace NesAsm.Emulator.AddressModes;

public class Indirect(CPU cpu) : AddressMode
{
    public override int Bytes => 1;
    public override byte GetValue() => throw new NotImplementedException();
    // TODO Extra cycle
    public override void SetValue(byte value) => throw new NotImplementedException();
    public override ushort GetAddress() => 0;
}
