using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BVC : Instruction
{
    public BVC(CPU cpu)
        : base(cpu, 0x50, "BVC", new Relative(cpu), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}




