namespace NesAsm.Emulator.AddressModes;

public class Relative(CPU cpu) : AddressMode
{
    public override byte GetValue()
    {
        var offset = (sbyte)cpu.NextByte();
        var address = (ushort)(cpu.PC + offset);

        return cpu.Memory.Read(address);
    }
    // TODO Extra cycle

    public override void SetValue(byte value) => throw new NotImplementedException();
}
