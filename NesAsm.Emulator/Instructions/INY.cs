using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record INY : Instruction
{
    public INY(CPU cpu)
        : base(cpu, 0xC8, "INY", new Implied(), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
