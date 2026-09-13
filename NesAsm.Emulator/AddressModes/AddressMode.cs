namespace NesAsm.Emulator.AddressModes;

public abstract class AddressMode
{
    public abstract byte GetValue();
    public abstract void SetValue(byte value);
}
