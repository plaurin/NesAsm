using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record PHA : Instruction
{
    public PHA(CPU cpu)
        : base(cpu, 0x48, "PHA", new Implied(), 3)
    {
    }

    public override void Execute()
    {
        Cpu.PushStack(Cpu.A);
    }
}
