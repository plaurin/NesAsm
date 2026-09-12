namespace NesAsm.Emulator;

public class NESEmulator
{
    private readonly byte[] _memory = new byte[64 * 1024];

    private byte _a;
    private byte _x;
    private byte _y;
    private byte _sp;

    private bool _carry;
    private bool _zero;
    private bool _negative;
    private bool _interrupt;
    private bool _overflow;
    private bool _decimal;

    public ReadOnlySpan<byte> Memory => _memory;
    public byte A => _a;
    public byte X => _x;
    public byte Y => _y;
    public byte SP => _sp;
    public bool Carry => _carry;
    public bool Zero => _zero;
    public bool Negative => _negative;
    public bool Interrupt => _interrupt;
    public bool Overflow => _overflow;
    public bool Decimal => _decimal;

    // --- Access ---

    // LDA
    public void LDAi(byte value) { _a = value; FlagNZ(_a); } // Immediate
    public void LDA(byte address) { _a = _memory[address]; FlagNZ(_a); } // Zero Page
    public void LDA(ushort address) { _a = _memory[address]; FlagNZ(_a); } // Absolute
    public void LDA(byte[] baseAddress, Register register) { _a = baseAddress[register]; FlagNZ(_a); } // Absolute Indexed (X or Y) CSharp

    // STA
    public void STA(byte address) => _memory[address] = _a; // Zero Page
    public void STA(ushort address) => _memory[address] = _a; // Absolute

    // LDX
    public void LDXi(byte value) { _x = value; FlagNZ(_x); } // Immediate
    public void LDX(byte address) { _x = _memory[address]; FlagNZ(_x); } // Zero Page
    public void LDX(ushort address) { _x = _memory[address]; FlagNZ(_x); } // Absolute

    // STX
    public void STX(byte address) => _memory[address] = _x; // Zero Page
    public void STX(ushort address) => _memory[address] = _x; // Absolute

    // LDY
    public void LDYi(byte value) { _y = value; FlagNZ(_y); } // Immediate
    public void LDY(byte address) { _y = _memory[address]; FlagNZ(_y); } // Zero Page
    public void LDY(ushort address) { _y = _memory[address]; FlagNZ(_y); } // Absolute

    // STY
    public void STY(byte address) => _memory[address] = _y; // Zero Page
    public void STY(ushort address) => _memory[address] = _y; // Absolute


    // --- Transfert ---

    public void TAX() { _x = _a; FlagNZ(_x); }
    public void TXA() { _a = _x; FlagNZ(_a); }
    public void TAY() { _y = _a; FlagNZ(_y); }
    public void TYA() { _a = _y; FlagNZ(_a); }


    // --- Arithmetic

    // ADC
    public void ADCi(byte value) // Immediate
    {
        var res = _a + value + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        // _overflow = (res ^ A) & (res ^ value) & 0x80; ??
        FlagNZC(res);
    }

    public void ADC(byte value) // Zero Page
    {
        var res = _a + _memory[value] + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        FlagNZC(res);
    }

    public void ADC(ushort value) // Absolute
    {
        var res = _a + _memory[value] + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        FlagNZC(res);
    }

    // SBC
    public void SBCi(byte value) // Immediate
    {
        var res = _a - value - (_carry ? 0 : 1);
        _a = (byte)(res & 0xFF);
        FlagNZC(res); // Need Sub!
    }

    // INC
    public void INC() => _a++;

    // INX
    public void INX() => _x++;

    // INY
    public void INY() => _y++;

    // DEX
    public void DEC() => _a--;

    // DEX
    public void DEX() => _x--;

    // DEY
    public void DEY() => _y--;

    // --- Shift ---

    // ASL
    public void ASL() { _carry = (_a >> 7) == 1; _a = (byte)(_a << 1); FlagNZ(_a); } // Accumulator

    /// <summary>
    /// Logical Shift Right. Shift one bit right in the Accumulator
    /// </summary>
    /// <remarks>Shifts the bits of the specified byte one bit to the right. The new value of bit #7 is zero. The old value of bit #0 is stored in the Carry flag.</remarks>
    public void LSR() { _carry = _a % 2 == 1; _negative = false; _a >>= 1; _zero = _a == 0; }

    public void LSR(ushort address) // Absolute
    {
        // Logical shift right on memory location
        var value = _memory[address];
        _carry = (value & 0x01) != 0;
        value >>= 1;
        _memory[address] = value;
        FlagNZ(value);
    } // memory operand

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

    public void ROR()
    {
        var oldCarry = _carry;
        _carry = (_a & 0x01) != 0;
        _a = (byte)((_a >> 1) | (oldCarry ? 0x80 : 0x00));
        FlagNZ(_a);
    }

    // --- Bitwise ---

    // AND
    public void ANDi(byte value) { _a &= value; FlagNZ(_a); } // Immediate
    public void AND(byte address) { _a &= _memory[address]; FlagNZ(_a); } // Zero Page

    // ORA
    public void ORA(byte address) { _a |= _memory[address]; _zero = _a == 0; _negative = _a >= 128; }

    // EOR
    public void EORi(byte value) { _a ^= value; FlagNZ(_a); } // Immediate
    public void EOR(byte address) { _a ^= _memory[address]; FlagNZ(_a); } // Zero Page

    // BIT
    public void BIT(ushort address)
    {
        var value = _memory[address];
        FlagNZ(value);
        _overflow = (value & 0x40) != 0;
    }


    // --- Compare ---

    // CMP
    public void CMPi(byte value) => CMPImpl(_a, value); // Immediate
    public void CMP(byte address) => CMPImpl(_a, _memory[address]); // Zero Page
    public void CMP(ushort address) => CMPImpl(_a, _memory[address]); // Absolute

    // CPX
    public void CPXi(byte value) => CMPImpl(_x, value); // Immediate
    public void CPX(byte address) => CMPImpl(_x, _memory[address]); // Zero Page
    public void CPX(ushort address) => CMPImpl(_x, _memory[address]); // Absolute

    // CPY
    public void CPYi(byte value) => CMPImpl(_y, value); // Immediate
    public void CPY(byte address) => CMPImpl(_y, _memory[address]); // Zero Page
    public void CPY(ushort address) => CMPImpl(_y, _memory[address]); // Absolute


    // --- Branch ---

    /// <summary>
    /// Branch on Carry clear
    /// </summary>
    /// <remarks>Only branches if the Carry flag is not set.</remarks>
    public bool BCC() => !_carry;

    /// <summary>
    /// Branch on Carry set
    /// </summary>
    /// <remarks>Only branches if the Carry flag is set.</remarks>
    public bool BCS() => _carry;

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
    /// Branch on result Plus
    /// </summary>
    /// <remarks>Only branches if the Negative flag is not set.</remarks>
    public bool BPL() => !_negative;

    /// <summary>
    /// Branch on result Minus
    /// </summary>
    /// <remarks>Only branches if the Negative flag is set.</remarks>
    public bool BMI() => _negative;

    /// <summary>
    /// Branch on Overflow clear
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is not set.</remarks>
    public bool BVC(/* Goto Label */) { return !_overflow; }

    /// <summary>
    /// Branch on Overflow set
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is set.</remarks>
    public bool BVS(/* Goto Label */)
    {
        return _overflow;
    }

    // --- Jump ---

    public void JMP(/* Goto Label */)
    {
        // No program counter in this emulator model; JMP is a control-flow operation and is a no-op here
    }

    public void JSR(/* nameof(Method) */)
    {
        // Push return address on stack would be typical; no PC/stack model here so treat as no-op
    }

    public void RTS()
    {
        // Return from subroutine: normally would pull return address from stack; no-op here
    }

    public void BRK()
    {
        // Force interrupt
        _interrupt = true;
    }

    public void RTI()
    {
        // Return from interrupt: normally would pull processor status and PC from stack; no-op here
    }


    // --- Stack ---

    public void PHA() { }
    public void PLA() { }
    public void PHP() { }
    public void PLP() { }
    public void TXS() { _sp = _x; }
    public void TSX() { _x = _sp; }


    // --- Flags ---

    public void CLC() { }
    public void SEC() { }
    public void CLI() { }
    /// <summary>
    /// Set Interrupt disable flag (I)
    /// </summary>
    public void SEI() => _interrupt = true;

    public void CLD() { _decimal = false; }
    public void SED() { _decimal = true; }
    public void CLV() { _overflow = false; }

    // --- Other ---
    public void NOP() { }


    // --- Private implementation ---

    private void CMPImpl(uint registerValue, int compareValue)
    {
        long result = registerValue - compareValue;

        _negative = (result & 0x80) > 0 && result != 0;
        _carry = result >= 0;
        _zero = result == 0;
    }

    private void FlagNZ(byte value)
    {
        _zero = value == 0;
        _negative = (value >> 7) == 1;
    }

    private void FlagNZC(int value)
    {
        _zero = value == 0;
        _negative = (value >> 7) == 1;
        _carry = value > 0xFF;
    }
}
