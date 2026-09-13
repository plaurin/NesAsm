using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BCS : Instruction
{
    public BCS(CPU cpu)
        : base(cpu, 0xB0, "BCS", new Relative(cpu), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}




