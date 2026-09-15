using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TAX : Instruction
{
    public TAX(CPU cpu)
        : base(cpu, 0xAA, "TAX", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetX_NZ(Cpu.A);
}
