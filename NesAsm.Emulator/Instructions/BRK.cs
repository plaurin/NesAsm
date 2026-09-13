using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BRK : Instruction
{
    public BRK(CPU cpu)
        : base(cpu, 0x00, "BRK", new Implied(), 7)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
