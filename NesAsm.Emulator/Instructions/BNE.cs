using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BNE : Instruction
{
    public BNE(CPU cpu)
        : base(cpu, 0xD0, "BNE", new Relative(cpu), 2)
    {
    }

    public override void Execute()
    {
        if (!Cpu.Zero)
        {
            Cpu.SetPC(AddressMode.GetAddress());
        }
    }
}
