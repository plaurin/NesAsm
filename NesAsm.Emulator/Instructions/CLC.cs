using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CLC : Instruction
{
    public CLC(CPU cpu)
        : base(cpu, 0x18, "CLC", new Implied(), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}

