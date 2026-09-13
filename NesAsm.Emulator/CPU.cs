using NesAsm.Emulator.AddressModes;
using NesAsm.Emulator.Instructions;

namespace NesAsm.Emulator;

public class CPU
{
    private readonly NesMemory _memory;

    private byte _a;
    private byte _x;
    private byte _y;
    private byte _sp;
    private ushort _pc;
    private long _cycles;

    private bool _carry;
    private bool _zero;
    private bool _negative;
    private bool _interrupt;
    private bool _overflow;
    private bool _decimal;

    public NesMemory Memory => _memory;

    public byte A => _a;
    public byte X => _x;
    public byte Y => _y;
    public byte SP => _sp;
    public ushort PC => _pc;
    public long Cycles => _cycles;
    public bool Carry => _carry;
    public bool Zero => _zero;
    public bool Negative => _negative;
    public bool Interrupt => _interrupt;
    public bool Overflow => _overflow;
    public bool Decimal => _decimal;

    public CPU(NesMemory memory)
    {
        _memory = memory;
        InitInstructions();
    }

    public void Init()
    {
        _cycles = 0;

        _a = 0;
        _x = 0;
        _y = 0;
        _sp = 0xFD;

        // Todo Flags
        //_pc =  (ushort)((_memory.Read(0xFFFD)))
    }

    // --------------
    public void SetA_NZ(byte value)
    {
        _a = value;
        FlagNZ(_a);
    }
    public void SetX_NZ(byte value)
    {
        _x = value;
        FlagNZ(_x);
    }
    public void SetY_NZ(byte value)
    {
        _y = value;
        FlagNZ(_y);
    }

    // --------------
    private readonly Instruction[] _instructionSet = new Instruction[256];
    private void InitInstructions()
    {
        var accumulator = new Accumulator(this);
        var immediate = new Immediate(this);
        var zeroPage = new ZeroPage(this);
        var zeroPageX = new ZeroPageX(this);
        var zeroPageY = new ZeroPageY(this);
        var absolute = new Absolute(this);
        var absoluteX = new AbsoluteX(this);
        var absoluteY = new AbsoluteY(this);
        var indirect = new  Indirect(this);
        var indirectX = new IndirectX(this);
        var indirectY = new IndirectY(this);

        void Init(Instruction instruction)
        {
            if (_instructionSet[instruction.Opcode] != null) throw new InvalidOperationException($"Already set {instruction.Opcode}");
            _instructionSet[instruction.Opcode] = instruction;
        }

        // Access
        Init(new LDA(this, 0xA9, immediate, 2));
        Init(new LDA(this, 0xA5, zeroPage, 3));
        Init(new LDA(this, 0xB5, zeroPageX, 4));
        Init(new LDA(this, 0xAD, absolute, 4));
        Init(new LDA(this, 0xBD, absoluteX, 4));
        Init(new LDA(this, 0xB9, absoluteY, 4));
        Init(new LDA(this, 0xA1, indirectX, 6));
        Init(new LDA(this, 0xA1, indirectY, 5));

        Init(new STA(this, 0x85, zeroPage, 3));
        Init(new STA(this, 0x95, zeroPageX, 4));
        Init(new STA(this, 0x8D, absolute, 4));
        Init(new STA(this, 0x9D, absoluteX, 5));
        Init(new STA(this, 0x99, absoluteY, 5));
        Init(new STA(this, 0x81, indirectX, 6));
        Init(new STA(this, 0x91, indirectY, 6));

        Init(new LDX(this, 0xA2, immediate, 2));
        Init(new LDX(this, 0xA6, zeroPage, 3));
        Init(new LDX(this, 0xB6, zeroPageY, 4));
        Init(new LDX(this, 0xAE, absolute, 4));
        Init(new LDX(this, 0xBE, absoluteY, 4));

        Init(new STX(this, 0x86, zeroPage, 3));
        Init(new STX(this, 0x96, zeroPageY, 4));
        Init(new STX(this, 0x8E, absolute, 4));

        Init(new LDY(this, 0xA0, immediate, 2));
        Init(new LDY(this, 0xA4, zeroPage, 3));
        Init(new LDY(this, 0xB4, zeroPageX, 4));
        Init(new LDY(this, 0xAC, absolute, 4));
        Init(new LDY(this, 0xBC, absoluteY, 4));

        Init(new STY(this, 0x84, zeroPage, 3));
        Init(new STY(this, 0x94, zeroPageX, 4));
        Init(new STY(this, 0x8C, absolute, 4));

        // Transfert
        Init(new TAX(this));
        Init(new TXA(this));
        Init(new TAY(this));
        Init(new TYA(this));

        // Arithmetic
        Init(new ADC(this, 0x69, immediate, 2));
        Init(new ADC(this, 0x65, zeroPage, 3));
        Init(new ADC(this, 0x75, zeroPageX, 4));
        Init(new ADC(this, 0x6D, absolute, 4));
        Init(new ADC(this, 0x7D, absoluteX, 4));
        Init(new ADC(this, 0x79, absoluteY, 4));
        Init(new ADC(this, 0x61, indirectX, 6));
        Init(new ADC(this, 0x71, indirectY, 5));

        Init(new SBC(this, 0xE9, immediate, 2));
        Init(new SBC(this, 0xE5, zeroPage, 3));
        Init(new SBC(this, 0xF5, zeroPageX, 4));
        Init(new SBC(this, 0xED, absolute, 4));
        Init(new SBC(this, 0xFD, absoluteX, 4));
        Init(new SBC(this, 0xF9, absoluteY, 4));
        Init(new SBC(this, 0xE1, indirectX, 6));
        Init(new SBC(this, 0xF1, indirectY, 5));

        Init(new INC(this, 0xE6, zeroPage, 5));
        Init(new INC(this, 0xF6, zeroPageX, 6));
        Init(new INC(this, 0xEE, absolute, 6));
        Init(new INC(this, 0xFE, absoluteX, 7));

        Init(new DEC(this, 0xC6, zeroPage, 5));
        Init(new DEC(this, 0xD6, zeroPageX, 6));
        Init(new DEC(this, 0xCE, absolute, 6));
        Init(new DEC(this, 0xDE, absoluteX, 7));

        Init(new INX(this)); // Implied
        Init(new DEX(this)); // Implied
        Init(new INY(this)); // Implied
        Init(new DEY(this)); // Implied

        // Shift
        Init(new ASL(this, 0x0A, accumulator, 2));
        Init(new ASL(this, 0x06, zeroPage, 5));
        Init(new ASL(this, 0x16, zeroPageX, 6));
        Init(new ASL(this, 0x0E, absolute, 6));
        Init(new ASL(this, 0x1E, absoluteX, 7));

        Init(new LSR(this, 0x4A, accumulator, 2));
        Init(new LSR(this, 0x46, zeroPage, 5));
        Init(new LSR(this, 0x56, zeroPageX, 6));
        Init(new LSR(this, 0x4E, absolute, 6));
        Init(new LSR(this, 0x5E, absoluteX, 7));

        Init(new ROL(this, 0x2A, accumulator, 2));
        Init(new ROL(this, 0x26, zeroPage, 5));
        Init(new ROL(this, 0x36, zeroPageX, 6));
        Init(new ROL(this, 0x2E, absolute, 6));
        Init(new ROL(this, 0x3E, absoluteX, 7));

        Init(new ROR(this, 0x6A, accumulator, 2));
        Init(new ROR(this, 0x66, zeroPage, 5));
        Init(new ROR(this, 0x76, zeroPageX, 6));
        Init(new ROR(this, 0x6E, absolute, 6));
        Init(new ROR(this, 0x7E, absoluteX, 7));

        // Bitwise
        Init(new AND(this, 0x29, immediate, 2));
        Init(new AND(this, 0x25, zeroPage, 3));
        Init(new AND(this, 0x35, zeroPageX, 4));
        Init(new AND(this, 0x2D, absolute, 4));
        Init(new AND(this, 0x3D, absoluteX, 4));
        Init(new AND(this, 0x39, absoluteY, 4));
        Init(new AND(this, 0x21, indirectX, 6));
        Init(new AND(this, 0x31, indirectY, 5));

        Init(new ORA(this, 0x09, immediate, 2));
        Init(new ORA(this, 0x05, zeroPage, 3));
        Init(new ORA(this, 0x15, zeroPageX, 4));
        Init(new ORA(this, 0x0D, absolute, 4));
        Init(new ORA(this, 0x1D, absoluteX, 4));
        Init(new ORA(this, 0x19, absoluteY, 4));
        Init(new ORA(this, 0x01, indirectX, 6));
        Init(new ORA(this, 0x11, indirectY, 5));

        Init(new EOR(this, 0x49, immediate, 2));
        Init(new EOR(this, 0x45, zeroPage, 3));
        Init(new EOR(this, 0x55, zeroPageX, 4));
        Init(new EOR(this, 0x4D, absolute, 4));
        Init(new EOR(this, 0x5D, absoluteX, 4));
        Init(new EOR(this, 0x59, absoluteY, 4));
        Init(new EOR(this, 0x41, indirectX, 6));
        Init(new EOR(this, 0x51, indirectY, 5));

        Init(new BIT(this, 0x24, zeroPage, 3));
        Init(new BIT(this, 0x2C, absolute, 4));

        // Compare
        Init(new CMP(this, 0xC9, immediate, 2));
        Init(new CMP(this, 0xC5, zeroPage, 3));
        Init(new CMP(this, 0xD5, zeroPageX, 4));
        Init(new CMP(this, 0xCD, absolute, 4));
        Init(new CMP(this, 0xDD, absoluteX, 4));
        Init(new CMP(this, 0xD9, absoluteY, 4));
        Init(new CMP(this, 0xC1, indirectX, 6));
        Init(new CMP(this, 0xD1, indirectY, 5));

        Init(new CPX(this, 0xE0, immediate, 2));
        Init(new CPX(this, 0xE4, zeroPage, 3));
        Init(new CPX(this, 0xEC, absolute, 4));

        Init(new CPY(this, 0xC0, immediate, 2));
        Init(new CPY(this, 0xC4, zeroPage, 3));
        Init(new CPY(this, 0xCC, absolute, 4));

        // Branch
        Init(new BCC(this)); // Relative
        Init(new BCS(this)); // Relative
        Init(new BEQ(this)); // Relative
        Init(new BNE(this)); // Relative
        Init(new BPL(this)); // Relative
        Init(new BMI(this)); // Relative
        Init(new BVC(this)); // Relative
        Init(new BVS(this)); // Relative

        // Jump
        Init(new JMP(this, 0x4C, absolute, 3));
        Init(new JMP(this, 0x6C, indirect, 5));

        Init(new RTS(this)); // Implied
        Init(new BRK(this)); // Implied
        Init(new RTI(this)); // Implied

        // Stack
        Init(new PHA(this)); // Implied
        Init(new PLA(this)); // Implied
        Init(new PHP(this)); // Implied
        Init(new PLP(this)); // Implied
        Init(new TXS(this)); // Implied
        Init(new TSX(this)); // Implied

        // Flags
        Init(new CLC(this)); // Implied
        Init(new SEC(this)); // Implied
        Init(new CLI(this)); // Implied
        Init(new SEI(this)); // Implied
        Init(new CLD(this)); // Implied
        Init(new SED(this)); // Implied
        Init(new CLV(this)); // Implied

        // Other
        Init(new NOP(this)); // Implied
    }


    public void RunNextInstruction()
    {
        var opcode = NextByte();

        switch (opcode)
        {
            case 0x00: BRK(); break;
            case 0x05: break; // Ins("ORA", 2, ZeroPage()),
            case 0x08: break; // Ins("PHP", 1, Implicit()),
            case 0x09: break; // Ins("ORA", 2, Immediate()),
            case 0x0A: break; // Ins("ASL", 1, Accumulator()),
            case 0x0E: break; // Ins("ASL", 3, Absolute()),
            case 0x10: break; // Ins("BPL", 2, Relative()),
            case 0x18: break; // Ins("CLC", 1, Implicit()),
            case 0x20: break; // Ins("JSR", 3, Absolute()),
            case 0x25: break; // Ins("AND", 2, ZeroPage()),
            case 0x29: break; // Ins("AND", 2, Immediate()),
            case 0x2A: break; // Ins("ROL", 1, Accumulator()),
            case 0x2C: break; // Ins("BIT", 3, Absolute()),
            case 0x2D: break; // Ins("AND", 3, Absolute()),
            case 0x2E: break; // Ins("ROL", 3, Absolute()),
            case 0x30: break; // Ins("BMI", 2, Relative()),
            case 0x38: break; // Ins("SEC", 1, Implicit()),
            case 0x3D: break; // Ins("AND", 3, AbsoluteX()),
            case 0x40: break; // Ins("RTI", 1, Implicit()),
            case 0x45: break; // Ins("EOR", 2, ZeroPage()),
            case 0x46: break; // Ins("LSR", 2, ZeroPage()),
            case 0x48: break; // Ins("PHA", 1, Implicit()),
            case 0x49: break; // Ins("EOR", 2, Immediate()),
            case 0x4A: break; // Ins("LSR", 1, Accumulator()),
            case 0x4C: break; // Ins("JMP", 3, Absolute()),
            case 0x60: break; // Ins("RTS", 1, Implicit()),
            case 0x65: break; // Ins("ADC", 2, ZeroPage()),
            case 0x68: break; // Ins("PLA", 1, Implicit()),
            case 0x69: break; // Ins("ADC", 2, Immediate()),
            case 0x6A: break; // Ins("ROR", 1, Accumulator()),
            case 0x6C: break; // Ins("JMP", 3, Indirect()), // Based on memory!! we need to emulate the memory to get the correct address
            case 0x6D: break; // Ins("ADC", 3, Absolute()),
            case 0x75: break; // Ins("ADC", 2, ZeroPageX()),
            case 0x78: break; // Ins("SEI", 1, Implicit()),
            case 0x79: break; // Ins("ADC", 3, AbsoluteY()),
            case 0x7D: break; // Ins("ADC", 3, AbsoluteX()),
            case 0x7E: break; // Ins("ROR", 3, AbsoluteX()),
            case 0x84: break; // Ins("STY", 2, ZeroPage()),
            case 0x85: break; // Ins("STA", 2, ZeroPage()),
            case 0x86: break; // Ins("STX", 2, ZeroPage()),
            case 0x88: break; // Ins("DEY", 1, Implicit()),
            case 0X8A: break; // Ins("TXA", 1, Implicit()),
            case 0x8C: break; // Ins("STY", 3, Absolute()),
            case 0x8D: break; // Ins("STA", 3, Absolute()),
            case 0x8E: break; // Ins("STX", 3, Absolute()),
            case 0x90: break; // Ins("BCC", 2, Relative()),
            case 0x91: break; // Ins("STA", 2, IndirectIndexed()),
            case 0x95: break; // Ins("STA", 2, ZeroPageX()),
            case 0x98: break; // Ins("TYA", 1, Implicit()),
            case 0x99: break; // Ins("STA", 3, AbsoluteY()),
            case 0x9A: break; // Ins("TXS", 1, Implicit()),
            case 0x9D: break; // Ins("STA", 3, AbsoluteX()),
            case 0xA0: break; // Ins("LDY", 2, Immediate()),
            case 0xA2: break; // Ins("LDX", 2, Immediate()),
            case 0xA4: break; // Ins("LDY", 2, ZeroPage()),
            case 0xA5: break; // Ins("LDA", 2, ZeroPage()),
            case 0xA6: break; // Ins("LDX", 2, ZeroPage()),
            case 0xA8: break; // Ins("TAY", 1, Implicit()),
            case 0xA9: break; // Ins("LDA", 2, Immediate()),
            case 0xAA: break; // Ins("TAX", 1, Implicit()),
            case 0xAC: break; // Ins("LDY", 3, Absolute()),
            case 0xAD: break; // Ins("LDA", 3, Absolute()),
            case 0xAE: break; // Ins("LDX", 3, Absolute()),
            case 0xB0: break; // Ins("BCS", 2, Relative()),
            case 0xB1: break; // Ins("LDA", 2, IndirectIndexed()),
            case 0xB5: break; // Ins("LDA", 2, ZeroPageX()),
            case 0xB9: break; // Ins("LDA", 3, AbsoluteY()),
            case 0xBD: break; // Ins("LDA", 3, AbsoluteX()),
            case 0xBE: break; // Ins("LDX", 3, AbsoluteY()),
            case 0xC0: break; // Ins("CPY", 2, Immediate()),
            case 0xC5: break; // Ins("CMP", 2, ZeroPage()),
            case 0xC6: break; // Ins("DEC", 2, ZeroPage()),
            case 0xC8: break; // Ins("INY", 1, Implicit()),
            case 0xC9: break; // Ins("CMP", 2, Immediate()),
            case 0xCA: break; // Ins("DEX", 1, Implicit()),
            case 0xCD: break; // Ins("CMP", 3, Absolute()),
            case 0xCE: break; // Ins("DEC", 3, Absolute()),
            case 0xD0: break; // Ins("BNE", 2, Relative()),
            case 0xD8: break; // Ins("CLD", 1, Implicit()),
            case 0xD9: break; // Ins("CMP", 3, AbsoluteY()),
            case 0xDD: break; // Ins("CMP", 3, AbsoluteX()),
            case 0xDE: break; // Ins("DEC", 3, AbsoluteX()),
            case 0xE0: break; // Ins("CPX", 2, Immediate()),
            case 0xE6: break; // Ins("INC", 2, ZeroPage()),
            case 0xE8: break; // Ins("INX", 1, Implicit()),
            case 0xE9: break; // Ins("SBC", 2, Immediate()),
            case 0xED: break; // Ins("SBC", 3, Absolute()),
            case 0xEE: break; // Ins("INC", 3, Absolute()),
            case 0xF0: break; // Ins("BEQ", 2, Relative()),
            case 0xF5: break; // Ins("SBC", 2, ZeroPageX()),
            case 0xF9: break; // Ins("SBC", 3, AbsoluteY()),
            default:
                break;
        }
    }

    public byte NextByte() => _memory.Read(_pc++);
    public ushort NextWord() => (ushort)(_memory.Read(_pc++) | (_memory.Read(_pc++) << 8));

    // --- Access ---

    // LDA
    public void LDA(Addressing addressing, int cycles) => LD(ref _a, addressing, cycles);
    public void LDX(Addressing addressing, int cycles) => LD(ref _x, addressing, cycles);
    public void LDY(Addressing addressing, int cycles) => LD(ref _y, addressing, cycles);

    public void LD(ref byte r, Addressing addressing, int cycles)
    {
        r = _memory.Read(addressing.Address);
        FlagNZ(r);
        _cycles += cycles + addressing.ExtraCycle;
    }

    public void LDAi(byte value) { _a = value; FlagNZ(_a); } // Immediate
    public void LDA(byte address) { _a = _memory.Read(address); FlagNZ(_a); } // Zero Page
    public void LDA(ushort address) { _a = _memory.Read(address); FlagNZ(_a); } // Absolute
    public void LDA(byte[] baseAddress, Register register) { _a = baseAddress[register]; FlagNZ(_a); } // Absolute Indexed (X or Y) CSharp

    // STA
    public void STA(byte address) => _memory.Write(address, _a); // Zero Page
    public void STA(ushort address) => _memory.Write(address, _a); // Absolute

    // LDX
    public void LDXi(byte value) { _x = value; FlagNZ(_x); } // Immediate
    public void LDX(byte address) { _x = _memory.Read(address); FlagNZ(_x); } // Zero Page
    public void LDX(ushort address) { _x = _memory.Read(address); FlagNZ(_x); } // Absolute

    // STX
    public void STX(byte address) => _memory.Write(address, _x); // Zero Page
    public void STX(ushort address) => _memory.Write(address, _x); // Absolute

    // LDY
    public void LDYi(byte value) { _y = value; FlagNZ(_y); } // Immediate
    public void LDY(byte address) { _y = _memory.Read(address); FlagNZ(_y); } // Zero Page
    public void LDY(ushort address) { _y = _memory.Read(address); FlagNZ(_y); } // Absolute

    // STY
    public void STY(byte address) => _memory.Write(address, _y); // Zero Page
    public void STY(ushort address) => _memory.Write(address, _y); // Absolute


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

    public void ADC(byte address) // Zero Page
    {
        var res = _a + _memory.Read(address) + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        FlagNZC(res);
    }

    public void ADC(ushort address) // Absolute
    {
        var res = _a + _memory.Read(address) + (_carry ? 1 : 0);
        _a = (byte)(res % 0xFF);
        FlagNZC(res);
    }

    public void ADCx(byte address) { var res = _a + _memory.Read((ushort)(address + _x)) + (_carry ? 1 : 0); _a = (byte)(res % 0xFF); FlagNZC(res); } // Zero Page X
    public void ADCy(byte address) { var res = _a + _memory.Read((ushort)(address + _y)) + (_carry ? 1 : 0); _a = (byte)(res % 0xFF); FlagNZC(res); } // Zero Page Y
    public void ADCx(ushort address) { var res = _a + _memory.Read((ushort)(address + _x)) + (_carry ? 1 : 0); _a = (byte)(res % 0xFF); FlagNZC(res); } // Absolute X
    public void ADCy(ushort address) { var res = _a + _memory.Read((ushort)(address + _y)) + (_carry ? 1 : 0); _a = (byte)(res % 0xFF); FlagNZC(res); } // Absolute Y

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
    public void ASL(ushort address) { byte res = _memory.Read(address); _carry = (res >> 7) == 1; res = (byte)(res << 1); _memory.Write(address, res); FlagNZ(res); } // Absolute

    /// <summary>
    /// Logical Shift Right. Shift one bit right in the Accumulator
    /// </summary>
    /// <remarks>Shifts the bits of the specified byte one bit to the right. The new value of bit #7 is zero. The old value of bit #0 is stored in the Carry flag.</remarks>
    public void LSR() { _carry = _a % 2 == 1; _negative = false; _a >>= 1; _zero = _a == 0; }

    public void LSR(ushort address) // Absolute
    {
        // Logical shift right on memory location
        var value = _memory.Read(address);
        _carry = (value & 0x01) != 0;
        value >>= 1;
        _memory.Write(address, value);
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
    public void ROL(ushort address) // Absolute
    {
        var res = _memory.Read(address);
        var c = _carry;
        _carry = (res & 0x80) > 0;
        res <<= 1;
        if (c) res += 1;
        _memory.Write(address, res);
        FlagNZ(res);
    }

    public void ROR() { var oldCarry = _carry; _carry = (_a & 0x01) != 0; _a = (byte)((_a >> 1) | (oldCarry ? 0x80 : 0x00)); FlagNZ(_a); } // Accumulator
    public void ROR(byte address) { var oldCarry = _carry; _carry = (_memory.Read(address) & 0x01) != 0; _a = (byte)((_memory.Read(address) >> 1) | (oldCarry ? 0x80 : 0x00)); FlagNZ(_a); } // Zero Page
    public void ROR(ushort address) { var oldCarry = _carry; _carry = (_memory.Read(address) & 0x01) != 0; _a = (byte)((_memory.Read(address) >> 1) | (oldCarry ? 0x80 : 0x00)); FlagNZ(_a); } // Absolute
    //public void RORx(byte address) { var oldCarry = _carry; _carry = (_memory[address + _x] & 0x01) != 0; _a = (byte)((_memory[address + _x] >> 1) | (oldCarry ? 0x80 : 0x00)); FlagNZ(_a); } // Zero Page
    //public void RORx(ushort address) { var oldCarry = _carry; _carry = (_memory[address + _x] & 0x01) != 0; _a = (byte)((_memory[address + _x] >> 1) | (oldCarry ? 0x80 : 0x00)); FlagNZ(_a); } // Absolute

    // --- Bitwise ---

    // AND
    public void ANDi(byte value) { _a &= value; FlagNZ(_a); } // Immediate
    public void AND(byte address) { _a &= _memory.Read(address); FlagNZ(_a); } // Zero Page
    public void AND(ushort address) { _a &= _memory.Read(address); FlagNZ(_a); } // Absolute
    //public void ANDax(ushort address) { _a &= _memory[address + _x]; FlagNZ(_a); } // Absolute

    // ORA
    public void ORA(Addressing addressing, int cycle)
    {
        //_a |= _memory.Read(address); FlagNZ(_a);

    }

    public void ORAi(byte value) { _a |= value; FlagNZ(_a); } // Immediate
    public void ORA(byte address) { _a |= _memory.Read(address); FlagNZ(_a); } // Zero Page

    // EOR
    public void EORi(byte value) { _a ^= value; FlagNZ(_a); } // Immediate
    public void EOR(byte address) { _a ^= _memory.Read(address); FlagNZ(_a); } // Zero Page

    // BIT
    public void BIT(ushort address)
    {
        var value = _memory.Read(address);
        FlagNZ(value);
        _overflow = (value & 0x40) != 0;
    }


    // --- Compare ---

    // CMP
    public void CMPi(byte value) => CMPImpl(_a, value); // Immediate
    public void CMP(byte address) => CMPImpl(_a, _memory.Read(address)); // Zero Page
    public void CMP(ushort address) => CMPImpl(_a, _memory.Read(address)); // Absolute

    // CPX
    public void CPXi(byte value) => CMPImpl(_x, value); // Immediate
    public void CPX(byte address) => CMPImpl(_x, _memory.Read(address)); // Zero Page
    public void CPX(ushort address) => CMPImpl(_x, _memory.Read(address)); // Absolute

    // CPY
    public void CPYi(byte value) => CMPImpl(_y, value); // Immediate
    public void CPY(byte address) => CMPImpl(_y, _memory.Read(address)); // Zero Page
    public void CPY(ushort address) => CMPImpl(_y, _memory.Read(address)); // Absolute


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


    // --- Addressing Modes ---

    private static Addressing Accumulator() => new(AddressingMode.Accumulator, 0, 0);
    private Addressing ZeroPage() => new(AddressingMode.ZeroPage, NextByte(), 0);
    private Addressing Absolute() => new(AddressingMode.Absolute, NextWord(), 0);


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
