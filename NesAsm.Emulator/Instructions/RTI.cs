using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record RTI : Instruction
{
    public RTI(CPU cpu)
        : base(cpu, 0x40, "RTI", new Implied(), 6)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
