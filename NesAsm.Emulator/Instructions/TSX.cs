using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TSX : Instruction
{
    public TSX(CPU cpu)
        : base(cpu, 0xBA, "TSX", new Implied(), 2)
    {
    }

    public override int Execute()
    {
        Cpu.SetX_NZ(Cpu.SP);

        return Cycles;
    }
}
