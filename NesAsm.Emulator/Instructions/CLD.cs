using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CLD : Instruction
{
    public CLD(CPU cpu)
        : base(cpu, 0xD8, "CLD", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetD(false);
}
