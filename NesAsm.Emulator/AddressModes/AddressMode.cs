namespace NesAsm.Emulator.AddressModes;

public abstract class AddressMode
{
    public abstract int Bytes { get; }
    public abstract byte GetValue();
    public abstract void SetValue(byte value);
    public abstract ushort GetAddress();
}
