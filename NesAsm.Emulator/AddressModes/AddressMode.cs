namespace NesAsm.Emulator.AddressModes;

public abstract class AddressMode
{
    public abstract int Bytes { get; }
    public abstract int GetExtraCycle(ushort cpuAddress);
    public abstract byte GetValue();
    public abstract void SetValue(byte value);
    public abstract ushort GetAddress();

    protected static int GetExtraCycle(ushort cpuAddress, ushort address) => (cpuAddress & 0xFF00) != (address & 0xFF00) ? 1 : 0;
}
