using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BVC : Instruction
{
    public BVC(CPU cpu)
        : base(cpu, 0x50, "BVC", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (!Cpu.Overflow)
        {
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
            Cpu.SetPC(AddressMode.GetAddress());
        }

        return Cycles + extraCycle;
    }
}
