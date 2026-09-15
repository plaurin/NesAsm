using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record PLP : Instruction
{
    public PLP(CPU cpu)
        : base(cpu, 0x28, "PLP", new Implied(), 4)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
