namespace NesAsm.Emulator;

public class ScriptBase
{
    private readonly NESEmulator _emulator;

    public ScriptBase(NESEmulator emulator)
    {
        _emulator = emulator;
    }

    public void LDAi(byte value) => _emulator.LDAi(value);
    public void LDXi(byte value) => _emulator.LDXi(value);
    public void LDYi(byte value) => _emulator.LDYi(value);
    public void INC() => _emulator.INC();
    public void INX() => _emulator.INX();
    public void INY() => _emulator.INY();
    public void DEC() => _emulator.DEC();
    public void DEX() => _emulator.DEX();
    public void DEY() => _emulator.DEY();

}
