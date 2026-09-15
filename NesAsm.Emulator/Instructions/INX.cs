using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record INX : Instruction
{
    public INX(CPU cpu)
        : base(cpu, 0xE8, "INX", new Implied(), 2)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
