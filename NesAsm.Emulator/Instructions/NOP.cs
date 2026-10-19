using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record NOP : Instruction
{
    public NOP(CPU cpu, byte opcode, AddressMode addressMode, int cycles, bool isOfficialOpCode = true)
        : base(cpu, opcode, "NOP", addressMode, cycles, isOfficialOpCode: isOfficialOpCode)
    {
    }

    public override int Execute()
    {
        return Cycles + AddressMode.GetExtraCycle(Cpu.PC);
    }
}

