namespace NesAsm.Emulator;

public class ScriptBase
{
    private readonly NESEmulator _emulator;

    public ScriptBase(NESEmulator emulator)
    {
        _emulator = emulator;
    }

    public void LDA(byte value) => _emulator.LDA(value);
    public void LDX(byte value) => _emulator.LDX(value);
    public void LDY(byte value) => _emulator.LDY(value);
    public void INC() => _emulator.INC();
    public void INX() => _emulator.INX();
    public void INY() => _emulator.INY();
    public void DEC() => _emulator.DEC();
    public void DEX() => _emulator.DEX();
    public void DEY() => _emulator.DEY();

}
