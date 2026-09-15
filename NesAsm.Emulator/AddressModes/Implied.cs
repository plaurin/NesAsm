namespace NesAsm.Emulator.AddressModes;

public class Implied() : AddressMode
{
    public override int Bytes => 0;
    public override byte GetValue() => 0;
    public override void SetValue(byte value) => throw new NotSupportedException();
    public override ushort GetAddress() => 0;
}
