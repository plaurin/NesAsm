using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record SEC : Instruction
{
    public SEC(CPU cpu)
        : base(cpu, 0x38, "SEC", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetC(true);
}
