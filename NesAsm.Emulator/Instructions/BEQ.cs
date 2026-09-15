using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BEQ : Instruction
{
    public BEQ(CPU cpu)
        : base(cpu, 0xF0, "BEQ", new Relative(cpu), 2)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
