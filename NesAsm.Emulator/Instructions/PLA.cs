using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record PLA : Instruction
{
    public PLA(CPU cpu)
        : base(cpu, 0x68, "PLA", new Implied(), 4)
    {
    }

    public override void Execute()
    {
        Cpu.SetA_NZ(Cpu.PopStack());
    }
}
