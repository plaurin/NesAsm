using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BPL : Instruction
{
    public BPL(CPU cpu)
        : base(cpu, 0x10, "BPL", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (!Cpu.Negative)
        {
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
            Cpu.SetPC(AddressMode.GetAddress());
        }

        return Cycles + extraCycle;
    }
}
