using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TSX : Instruction
{
    public TSX(CPU cpu)
        : base(cpu, 0xBA, "TSX", new Implied(), 2)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
