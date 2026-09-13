using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BCC : Instruction
{
    public BCC(CPU cpu)
        : base(cpu, 0x90, "BCC", new Relative(cpu), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}