using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TAY : Instruction
{
    public TAY(CPU cpu)
        : base(cpu, 0xA8, "TAY", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetY_NZ(Cpu.A);
}
