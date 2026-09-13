using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CLI : Instruction
{
    public CLI(CPU cpu)
        : base(cpu, 0x58, "CLI", new Implied(), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}

