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
        Init(new LDA(this, 0xB1, indirectY, 5));

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

        var instruction = _instructionSet[opcode];

        instruction.Run();
    }

    public byte NextByte() => _memory.Read(_pc++);
    public ushort NextWord() => (ushort)(_memory.Read(_pc++) | (_memory.Read(_pc++) << 8));

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
