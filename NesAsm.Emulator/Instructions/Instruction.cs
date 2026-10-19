using NesAsm.Emulator.AddressModes;
using System.Text;

namespace NesAsm.Emulator.Instructions;

public abstract record Instruction
{
    protected Instruction(CPU cpu, byte opcode, string mnemonic, AddressMode addressMode, int cycles, int? bytesOverride = null, bool isOfficialOpCode = true)
    {
        Bytes = bytesOverride ?? 1 + addressMode.Bytes;
        IsOfficialOpCode = isOfficialOpCode;
        Cpu = cpu;
        Opcode = opcode;
        Mnemonic = mnemonic;
        AddressMode = addressMode;
        Cycles = cycles;
    }

    public int Bytes { get; }
    public bool IsOfficialOpCode { get; }
    public CPU Cpu { get; }
    public byte Opcode { get; }
    public string Mnemonic { get; }
    public AddressMode AddressMode { get; }
    public int Cycles { get; }

    public abstract int Execute();

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"0x{Opcode:X2} {AddressMode.GetType().Name} {Bytes} bytes, {Cycles} cycles");
        return true;
    }
}
