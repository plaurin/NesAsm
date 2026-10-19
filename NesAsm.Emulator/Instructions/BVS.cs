using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BVS : Instruction
{
    public BVS(CPU cpu)
        : base(cpu, 0x70, "BVS", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (Cpu.Overflow)
        {
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
            Cpu.SetPC(AddressMode.GetAddress());
        }

        return Cycles + extraCycle;
    }
}