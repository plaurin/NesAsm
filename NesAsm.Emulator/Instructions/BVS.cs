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
            Cpu.SetPC(AddressMode.GetAddress());
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
        }

        return Cycles + extraCycle;
    }
}