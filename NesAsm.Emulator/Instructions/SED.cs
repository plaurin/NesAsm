using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record SED : Instruction
{
    public SED(CPU cpu)
        : base(cpu, 0xF8, "SED", new Implied(), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}

