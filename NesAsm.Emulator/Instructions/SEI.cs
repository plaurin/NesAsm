using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record SEI : Instruction
{
    public SEI(CPU cpu)
        : base(cpu, 0x78, "SEI", new Implied(), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}

