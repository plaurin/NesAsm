namespace NesAsm.Emulator;

public class ScriptBase
{
    private readonly NESEmulator _emulator;

    public ScriptBase(NESEmulator emulator)
    {
        _emulator = emulator;
    }

    protected void Call<T>(Action<T> action) where T : ScriptBase
    {
        var ctor = typeof(T).GetConstructors().First();
        var instance = (T)ctor.Invoke(new[] { _emulator });
        action.Invoke(instance);
    }

    public void LDAi(byte value) => _emulator.LDAi(value);
    public void LDXi(byte value) => _emulator.LDXi(value);
    public void LDYi(byte value) => _emulator.LDYi(value);

    public void LDAa(byte argumentValue) => _emulator.LDAi(argumentValue);
    public void LDXa(byte argumentValue) => _emulator.LDXi(argumentValue);
    public void LDYa(byte argumentValue) => _emulator.LDYi(argumentValue);

    public void LDAa(bool argumentValue) => _emulator.LDAi((byte)(argumentValue ? 1 : 0));
    public void LDXa(bool argumentValue) => _emulator.LDXi((byte)(argumentValue ? 1 : 0));
    public void LDYa(bool argumentValue) => _emulator.LDYi((byte)(argumentValue ? 1 : 0));

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
