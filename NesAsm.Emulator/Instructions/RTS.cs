using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record RTS : Instruction
{
    public RTS(CPU cpu)
        : base(cpu, 0x60, "RTS", new Implied(), 6)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}