namespace NesAsm.Emulator;

public static class NESEmulatorStatic
{
    private static readonly byte[] _memory = new byte[64 * 1024];

    private static byte _a;
    private static byte _x;
    private static byte _y;

    private static bool _carry;
    private static bool _zero;
    private static bool _negative;

    public static ReadOnlySpan<byte> Memory => _memory;
    public static byte A => _a;
    public static byte X => _x;
    public static byte Y => _y;
    public static bool Carry => _carry;
    public static bool Zero => _zero;
    public static bool Negative => _negative;

    // Immediate Load instructions
    public static void LDAi(byte value) => _a = value;
    public static void LDXi(byte value) => _x = value;
    public static void LDYi(byte value) => _y = value;

    // Increase and Decrease X/Y register instructions
    public static void INX() => _x++;
    public static void INY() => _y++;

    public static void DEX() => _x--;
    public static void DEY() => _y--;

    // Transfert instructions
    public static void TAY() => _y = _a;
    public static void TYA() => _a = _y;
    public static void TAX() => _x = _a;
    public static void TXA() => _a = _x;

    public static void TXS() { throw new NotImplementedException(); } // SP = X

    // Zero Page Store instructions
    public static void STA(byte address) => _memory[address] = _a;
    public static void STX(byte address) => _memory[address] = _x;
    public static void STY(byte address) => _memory[address] = _y;

    // Zero Page Load instructions
    public static void LDA(byte address) => _a = _memory[address];
    public static void LDX(byte address) => _x = _memory[address];
    public static void LDY(byte address) => _y = _memory[address];

    public static void STA(ushort baseAddress, Register register) { /* TODO */ }

    // Absolute Mode Store instructions
    public static void STA(ushort address) => _memory[address] = _a;
    public static void STX(ushort address) => _memory[address] = _x;
    public static void STY(ushort address) => _memory[address] = _y;

    // Absolute Mode Load instructions
    public static void LDA(ushort address) => _a = _memory[address];
    public static void LDX(ushort address) => _x = _memory[address];
    public static void LDY(ushort address) => _y = _memory[address];

    // Absolute Indexed Mode Load instructions
    public static void LDA(byte[] baseAddress, Register register) => _a = baseAddress[register];

    // Immediate Compare instructions
    public static void CMPi(byte value) => CMPImpl(_a, value);
    public static void CPXi(byte value) => CMPImpl(_x, value);
    public static void CPYi(byte value) => CMPImpl(_y, value);

    // Zero Page Compare instructions
    public static void CMP(byte address) => CMPImpl(_a, _memory[address]);
    public static void CPX(byte address) => CMPImpl(_x, _memory[address]);
    public static void CPY(byte address) => CMPImpl(_y, _memory[address]);

    // Absolute Compare instructions
    public static void CMP(ushort address) => CMPImpl(_a, _memory[address]);
    public static void CPX(ushort address) => CMPImpl(_x, _memory[address]);
    public static void CPY(ushort address) => CMPImpl(_y, _memory[address]);

    private static void CMPImpl(uint registerValue, int compareValue)
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
    public static bool BMI() => _negative;

    /// <summary>
    /// Branch on result Plus
    /// </summary>
    /// <remarks>Only branches if the Negative flag is not set.</remarks>
    public static bool BPL() => !_negative;

    /// <summary>
    /// Branch on Overflow set
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is set.</remarks>
    public static void BVS(/* Goto Label */) { throw new NotImplementedException(); }

    /// <summary>
    /// Branch on Overflow clear
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is not set.</remarks>
    public static void BVC(/* Goto Label */) { throw new NotImplementedException(); }

    /// <summary>
    /// Branch on Carry set
    /// </summary>
    /// <remarks>Only branches if the Carry flag is set.</remarks>
    public static bool BCS() => _carry;

    /// <summary>
    /// Branch on Carry clear
    /// </summary>
    /// <remarks>Only branches if the Carry flag is not set.</remarks>
    public static bool BCC() => !_carry;

    /// <summary>
    /// Branch on result Zero
    /// </summary>
    /// <remarks>Only branches if the Zero flag is set. Usage: if(nesEmulator.BEQ()) goto label;</remarks>
    public static bool BEQ() => _zero;

    /// <summary>
    /// Branch on result not Zero
    /// </summary>
    /// <remarks>Only branches if the Zero flag is not set.</remarks>
    public static bool BNE() => !_zero;

    /// <summary>
    /// Logical Shift Right. Shift one bit right in the Accumulator
    /// </summary>
    /// <remarks>Shifts the bits of the specified byte one bit to the right. The new value of bit #7 is zero. The old value of bit #0 is stored in the Carry flag.</remarks>
    public static void LSR() { _carry = _a % 2 == 1; _negative = false; _a >>= 1; _zero = _a == 0; }

    public static void LSR(ushort address) { throw new NotImplementedException(); } // ??? operand?

    /// <summary>
    /// Rotate one bit left in the Accumulator
    /// </summary>
    /// <remarks>Rotates the bits of the specified byte by one bit to the left. The new value of bit #0 comes from the Carry flag, and then the old value of bit #7 is used to update the Carry flag.</remarks>
    public static void ROL() { var c = _carry; _carry = (_a & 0x80) > 0; _a <<= 1; if (c) _a += 1; }

    /// <summary>
    /// Rotate one bit left in Memory
    /// </summary>
    /// <remarks>Rotates the bits of the specified byte by one bit to the left. The new value of bit #0 comes from the Carry flag, and then the old value of bit #7 is used to update the Carry flag.</remarks>
    public static void ROL(ushort address) { var c = _carry; _carry = (_memory[address] & 0x80) > 0; _memory[address] <<= 1; if (c) _memory[address] += 1; }

    public static void JMP(/* Goto Label */) { throw new NotImplementedException(); }

    public static void BIT(ushort address) { throw new NotImplementedException(); }
    
    public static void SEI() { throw new NotImplementedException(); }

    public static void CLD() { throw new NotImplementedException(); }

    public static void JSR(/* nameof(Method) */) { throw new NotImplementedException(); }
    public static void RTI() { throw new NotImplementedException(); }
    public static void RTS() { throw new NotImplementedException(); }
}
