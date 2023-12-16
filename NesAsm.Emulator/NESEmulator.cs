namespace NesAsm.Emulator;

public class NESEmulator
{
    private readonly byte[] _memory = new byte[64 * 1024];

    private byte _a;
    private byte _x;
    private byte _y;

    public ReadOnlySpan<byte> Memory => _memory;
    public byte A => _a;
    public byte X => _x;
    public byte Y => _y;

    public void LDA(byte value) => _a = value;
    public void LDX(byte value) => _x = value;
    public void LDY(byte value) => _y = value;

    public void INC() => _a++;
    public void INX() => _x++;
    public void INY() => _y++;

    public void DEC() => _a--;
    public void DEX() => _x--;
    public void DEY() => _y--;

    public void TAY() => _y = _a;
    public void TYA() => _a = _y;
    public void TAX() => _x = _a;
    public void TXA() => _a = _x;
}
