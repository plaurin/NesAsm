using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TYA : Instruction
{
    public TYA(CPU cpu)
        : base(cpu, 0x98, "TYA", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetA_NZ(Cpu.Y);
}
