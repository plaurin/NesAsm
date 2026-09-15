using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record PHP : Instruction
{
    public PHP(CPU cpu)
        : base(cpu, 0x08, "PHP", new Implied(), 3)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
