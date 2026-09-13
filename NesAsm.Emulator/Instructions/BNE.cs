using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BNE : Instruction
{
    public BNE(CPU cpu)
        : base(cpu, 0xD0, "BNE", new Relative(cpu), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}




