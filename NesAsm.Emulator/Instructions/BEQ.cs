using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BEQ : Instruction
{
    public BEQ(CPU cpu)
        : base(cpu, 0xF0, "BEQ", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (Cpu.Zero)
        {
            Cpu.SetPC(AddressMode.GetAddress());
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
        }

        return Cycles + extraCycle;
    }
}
