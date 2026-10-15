using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record LDA : Instruction
{
    public LDA(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "LDA", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetA_NZ(value);

        return Cycles + AddressMode.GetExtraCycle(Cpu.PC);
    }
}
