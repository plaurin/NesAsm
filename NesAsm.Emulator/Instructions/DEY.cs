using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record DEY : Instruction
{
    public DEY(CPU cpu)
        : base(cpu, 0x88, "DEY", new Implied(), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
