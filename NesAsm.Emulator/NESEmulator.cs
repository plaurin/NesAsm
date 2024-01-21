namespace NesAsm.Emulator;

public class NESEmulator
{
    private readonly byte[] _memory = new byte[64 * 1024];

    private byte _a;
    private byte _x;
    private byte _y;

    private bool _carry;
    private bool _zero;
    private bool _negative;

    public ReadOnlySpan<byte> Memory => _memory;
    public byte A => _a;
    public byte X => _x;
    public byte Y => _y;
    public bool Carry => _carry;
    public bool Zero => _zero;
    public bool Negative => _negative;

    // Immediate Load instructions
    public void LDAi(byte value) => _a = value;
    public void LDXi(byte value) => _x = value;
    public void LDYi(byte value) => _y = value;

    // Increase and Decrease X/Y register instructions
    public void INX() => _x++;
    public void INY() => _y++;

    public void DEX() => _x--;
    public void DEY() => _y--;

    // Transfert instructions
    public void TAY() => _y = _a;
    public void TYA() => _a = _y;
    public void TAX() => _x = _a;
    public void TXA() => _a = _x;

    public void TXS() { throw new NotImplementedException(); } // SP = X

    // Zero Page Store instructions
    public void STA(byte address) => _memory[address] = _a;
    public void STX(byte address) => _memory[address] = _x;
    public void STY(byte address) => _memory[address] = _y;

    // Zero Page Load instructions
    public void LDA(byte address) => _a = _memory[address];
    public void LDX(byte address) => _x = _memory[address];
    public void LDY(byte address) => _y = _memory[address];

    // Absolute Mode Store instructions
    public void STA(ushort address) => _memory[address] = _a;
    public void STX(ushort address) => _memory[address] = _x;
    public void STY(ushort address) => _memory[address] = _y;

    // Absolute Mode Load instructions
    public void LDA(ushort address) => _a = _memory[address];
    public void LDX(ushort address) => _x = _memory[address];
    public void LDY(ushort address) => _y = _memory[address];

    // Absolute Indexed Mode Load instructions
    public void LDA(byte[] baseAddress, AddressingRegister register) => _a = baseAddress[register == AddressingRegister.X ? _x : _y];

    // Immediate Compare instructions
    public void CMPi(byte value) => CMPImpl(_a, value);
    public void CPXi(byte value) => CMPImpl(_x, value);
    public void CPYi(byte value) => CMPImpl(_y, value);

    // Zero Page Compare instructions
    public void CMP(byte address) => CMPImpl(_a, _memory[address]);
    public void CPX(byte address) => CMPImpl(_x, _memory[address]);
    public void CPY(byte address) => CMPImpl(_y, _memory[address]);

    // Absolute Compare instructions
    public void CMP(ushort address) => CMPImpl(_a, _memory[address]);
    public void CPX(ushort address) => CMPImpl(_x, _memory[address]);
    public void CPY(ushort address) => CMPImpl(_y, _memory[address]);

    private void CMPImpl(uint registerValue, int compareValue)
    {
        long result = registerValue - compareValue;

        _negative = (result & 0x80) > 0 && result != 0;
        _carry = result >= 0;
        _zero = result == 0;
    }

    // Branching instructions

    /// <summary>
    /// Branch on result Minus
    /// </summary>
    /// <remarks>Only branches if the Negative flag is set.</remarks>
    public bool BMI() => _negative;

    /// <summary>
    /// Branch on result Plus
    /// </summary>
    /// <remarks>Only branches if the Negative flag is not set.</remarks>
    public bool BPL() => !_negative;

    /// <summary>
    /// Branch on Overflow set
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is set.</remarks>
    public void BVS(/* Goto Label */) { throw new NotImplementedException(); }

    /// <summary>
    /// Branch on Overflow clear
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is not set.</remarks>
    public void BVC(/* Goto Label */) { throw new NotImplementedException(); }

    /// <summary>
    /// Branch on Carry set
    /// </summary>
    /// <remarks>Only branches if the Carry flag is set.</remarks>
    public bool BCS() => _carry;

    /// <summary>
    /// Branch on Carry clear
    /// </summary>
    /// <remarks>Only branches if the Carry flag is not set.</remarks>
    public bool BCC() => !_carry;

    /// <summary>
    /// Branch on result Zero
    /// </summary>
    /// <remarks>Only branches if the Zero flag is set. Usage: if(nesEmulator.BEQ()) goto label;</remarks>
    public bool BEQ() => _zero;

    /// <summary>
    /// Branch on result not Zero
    /// </summary>
    /// <remarks>Only branches if the Zero flag is not set.</remarks>
    public bool BNE() => !_zero;

    /// <summary>
    /// Logical Shift Right. Shift one bit right in the Accumulator
    /// </summary>
    /// <remarks>Shifts the bits of the specified byte one bit to the right. The new value of bit #7 is zero. The old value of bit #0 is stored in the Carry flag.</remarks>
    public void LSR() { _carry = _a % 2 == 1; _negative = false; _a >>= 1; _zero = _a == 0; }

    public void LSR(ushort address) { throw new NotImplementedException(); } // ??? operand?

    /// <summary>
    /// Rotate one bit left in the Accumulator
    /// </summary>
    /// <remarks>Rotates the bits of the specified byte by one bit to the left. The new value of bit #0 comes from the Carry flag, and then the old value of bit #7 is used to update the Carry flag.</remarks>
    public void ROL() { var c = _carry; _carry = (_a & 0x80) > 0; _a <<= 1; if (c) _a += 1; }

    /// <summary>
    /// Rotate one bit left in Memory
    /// </summary>
    /// <remarks>Rotates the bits of the specified byte by one bit to the left. The new value of bit #0 comes from the Carry flag, and then the old value of bit #7 is used to update the Carry flag.</remarks>
    public void ROL(ushort address) { var c = _carry; _carry = (_memory[address] & 0x80) > 0; _memory[address] <<= 1; if (c) _memory[address] += 1; }

    public void JMP(/* Goto Label */) { throw new NotImplementedException(); }

    public void BIT(ushort address) { throw new NotImplementedException(); }
    
    public void SEI() { throw new NotImplementedException(); }

    public void CLD() { throw new NotImplementedException(); }

    public void JSR(/* nameof(Method) */) { throw new NotImplementedException(); }
    public void RTI() { throw new NotImplementedException(); }
    public void RTS() { throw new NotImplementedException(); }
}
