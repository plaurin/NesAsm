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
    public void LDAi(byte value) => _a = value; // Immediate
    public void LDA(byte address) => _a = _memory[address]; // Zero Page
    public void LDA(ushort address) => _a = _memory[address]; // Absolute
    public void LDA(byte[] baseAddress, Register register) => _a = baseAddress[register]; // Absolute Indexed (X or Y)

    // STA
    public void STA(byte address) => _memory[address] = _a; // Zero Page
    public void STA(ushort address) => _memory[address] = _a; // Absolute

    // LDX
    public void LDXi(byte value) => _x = value; // Immediate
    public void LDX(byte address) => _x = _memory[address]; // Zero Page
    public void LDX(ushort address) => _x = _memory[address]; // Absolute

    // STX
    public void STX(byte address) => _memory[address] = _x; // Zero Page
    public void STX(ushort address) => _memory[address] = _x; // Absolute

    // LDY
    public void LDYi(byte value) => _y = value; // Immediate
    public void LDY(byte address) => _y = _memory[address]; // Zero Page
    public void LDY(ushort address) => _y = _memory[address]; // Absolute

    // STY
    public void STY(byte address) => _memory[address] = _y; // Zero Page
    public void STY(ushort address) => _memory[address] = _y; // Absolute


    // --- Transfert ---

    public void TAX() => _x = _a;
    public void TXA() => _a = _x;
    public void TAY() => _y = _a;
    public void TYA() => _a = _y;


    // --- Arithmetic

    // ADC
    public void ADCi(byte value)
    {
        var res = _a + value + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        _carry = res > 0xFF;
        _zero = _a == 0;
        // _overflow = (res ^ A) & (res ^ value) & 0x80; ??
        _negative = _a >= 0x80;
    }

    public void ADC(byte value) // Zero Page
    {
        var res = _a + _memory[value] + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        _carry = res > 0xFF;
        _zero = _a == 0;
        // _overflow = (res ^ A) & (res ^ value) & 0x80; ??
        _negative = _a >= 0x80;
    }

    public void ADC(ushort value) // Absolute
    {
        var res = _a + _memory[value] + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        _carry = res > 0xFF;
        _zero = _a == 0;
        // _overflow = (res ^ A) & (res ^ value) & 0x80; ??
        _negative = _a >= 0x80;
    }

    // INX
    public void INX() => _x++;

    // INY
    public void INY() => _y++;

    // DEX
    public void DEX() => _x--;

    // DEY
    public void DEY() => _y--;


    // --- Shift ---

    // LSR

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
        _negative = (value & 0x80) != 0;
        _zero = value == 0;
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

    // --- Bitwise ---

    public void ORA(byte address) { _a |= _memory[address]; _zero = _a == 0; _negative = _a >= 128; }

    public void BIT(ushort address)
    {
        var value = _memory[address];
        _zero = (_a & value) == 0;
        _negative = (value & 0x80) != 0;
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
    public void BVC(/* Goto Label */)
    {
        // Branch on overflow clear - no PC in this emulator stub
    }

    /// <summary>
    /// Branch on Overflow set
    /// </summary>
    /// <remarks>Only branches if the Overflow flag is set.</remarks>
    public void BVS(/* Goto Label */)
    {
        // Branch on overflow set - no PC in this emulator stub, just leave flags available
        // This method exists to reflect the instruction; actual branching handled by caller using Overflow property
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

    public void TXS() { _sp = _x; }


    // --- Flags ---

    /// <summary>
    /// Set Interrupt disable flag (I)
    /// </summary>
    public void SEI() => _interrupt = true;

    public void CLD() { _decimal = false; }

    // --- Other ---


    // --- Private implementation ---

    private void CMPImpl(uint registerValue, int compareValue)
    {
        long result = registerValue - compareValue;

        _negative = (result & 0x80) > 0 && result != 0;
        _carry = result >= 0;
        _zero = result == 0;
    }
}
