using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BVS : Instruction
{
    public BVS(CPU cpu)
        : base(cpu, 0x70, "BVS", new Relative(cpu), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}




