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
    public void INX() => _emulator.INX();
    public void INY() => _emulator.INY();
    public void DEX() => _emulator.DEX();
    public void DEY() => _emulator.DEY();

    public void STA(byte address) => _emulator.STA(address);
    public void STX(byte address) => _emulator.STX(address);
    public void STY(byte address) => _emulator.STY(address);

    public void CMPi(byte value) => _emulator.CMPi(value);
    public void CPXi(byte value) => _emulator.CPXi(value);
    public void CPYi(byte value) => _emulator.CPYi(value);
    public void CMP(byte address) => _emulator.CMP(address);
    public void CPX(byte address) => _emulator.CPX(address);
    public void CPY(byte address) => _emulator.CPY(address);
    public void CMP(ushort address) => _emulator.CMP(address);
    public void CPX(ushort address) => _emulator.CPX(address);
    public void CPY(ushort address) => _emulator.CPY(address);

    public bool BMI() => _emulator.BMI();
    public bool BPL() => _emulator.BPL();
    public bool BCS() => _emulator.BCS();
    public bool BCC() => _emulator.BCC();
    public bool BEQ() => _emulator.BEQ();
    public bool BNE() => _emulator.BNE();
}
