using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BNE : Instruction
{
    public BNE(CPU cpu)
        : base(cpu, 0xD0, "BNE", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (!Cpu.Zero)
        {
            Cpu.SetPC(AddressMode.GetAddress());
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
        }

        return Cycles + extraCycle;
    }
}
