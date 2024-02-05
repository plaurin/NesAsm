namespace NesAsm.Emulator;

public abstract partial class ScriptBase
{
    private readonly NESEmulator _emulator;

    protected ScriptBase(NESEmulator emulator)
    {
        _emulator = emulator;
    }

    protected void Call<T>(Action<T> action) where T : ScriptBase
    {
        var ctor = typeof(T).GetConstructors().First();
        var instance = (T)ctor.Invoke(new[] { _emulator });
        action.Invoke(instance);
    }

    protected void Jump<T>(Action<T> action) where T : ScriptBase
    {
        var ctor = typeof(T).GetConstructors().First();
        var instance = (T)ctor.Invoke(new[] { _emulator });
        action.Invoke(instance);
    }

    protected Register A = new();
    protected Register X = new();
    protected Register Y = new();

    protected void LDAi(byte value) => _emulator.LDAi(value);
    protected void LDXi(byte value) => _emulator.LDXi(value);
    protected void LDYi(byte value) => _emulator.LDYi(value);

    protected void LDA(byte address) => _emulator.LDA(address);
    protected void LDX(byte address) => _emulator.LDX(address);
    protected void LDY(byte address) => _emulator.LDY(address);

    protected void LDA(ushort address) => _emulator.LDA(address);
    protected void LDX(ushort address) => _emulator.LDX(address);
    protected void LDY(ushort address) => _emulator.LDY(address);

    protected void LDAa(byte argumentValue) => _emulator.LDAi(argumentValue);
    protected void LDXa(byte argumentValue) => _emulator.LDXi(argumentValue);
    protected void LDYa(byte argumentValue) => _emulator.LDYi(argumentValue);

    protected void LDAa(bool argumentValue) => _emulator.LDAi((byte)(argumentValue ? 1 : 0));
    protected void LDXa(bool argumentValue) => _emulator.LDXi((byte)(argumentValue ? 1 : 0));
    protected void LDYa(bool argumentValue) => _emulator.LDYi((byte)(argumentValue ? 1 : 0));

    protected void LDA(byte[] baseAddress, Register register) => _emulator.LDA(baseAddress, register);

    protected void INC(ushort address) { throw new NotImplementedException(); }
    protected void INX() => _emulator.INX();
    protected void INY() => _emulator.INY();
    protected void DEC(ushort address) { throw new NotImplementedException(); }
    protected void DEX() => _emulator.DEX();
    protected void DEY() => _emulator.DEY();

    protected void STA(byte address) => _emulator.STA(address);
    protected void STX(byte address) => _emulator.STX(address);
    protected void STY(byte address) => _emulator.STY(address);

    protected void STA(ushort address) => _emulator.STA(address);
    protected void STX(ushort address) => _emulator.STX(address);
    protected void STY(ushort address) => _emulator.STY(address);

    protected void STA(ushort baseAddress, Register register) { }

    protected void CMPi(byte value) => _emulator.CMPi(value);
    protected void CPXi(byte value) => _emulator.CPXi(value);
    protected void CPYi(byte value) => _emulator.CPYi(value);
    protected void CMP(byte address) => _emulator.CMP(address);
    protected void CPX(byte address) => _emulator.CPX(address);
    protected void CPY(byte address) => _emulator.CPY(address);
    protected void CMP(ushort address) => _emulator.CMP(address);
    protected void CPX(ushort address) => _emulator.CPX(address);
    protected void CPY(ushort address) => _emulator.CPY(address);

    protected bool BMI() => _emulator.BMI();
    protected bool BPL() => _emulator.BPL();
    protected bool BCS() => _emulator.BCS();
    protected bool BCC() => _emulator.BCC();
    protected bool BEQ() => _emulator.BEQ();
    protected bool BNE() => _emulator.BNE();

    protected void ANDi(byte value) { throw new NotImplementedException(); }

    protected void LSR() => _emulator.LSR();
    protected void ROL() => _emulator.ROL();
    protected void ROL(ushort address) => _emulator.ROL(address);

    protected void SEI() { throw new NotImplementedException(); }
    protected void CLD() { throw new NotImplementedException(); }
    protected void TXS() { throw new NotImplementedException(); }
    protected void BIT(ushort address) { throw new NotImplementedException(); }

}
