using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BPL : Instruction
{
    public BPL(CPU cpu)
        : base(cpu, 0x10, "BPL", new Relative(cpu), 2)
    {
    }

    public override void Execute()
    {
        if (!Cpu.Negative)
        {
            Cpu.SetPC(AddressMode.GetAddress());
        }
    }
}
