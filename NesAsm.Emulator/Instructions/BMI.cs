using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BMI : Instruction
{
    public BMI(CPU cpu)
        : base(cpu, 0x30, "BMI", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (Cpu.Negative)
        {
            Cpu.SetPC(AddressMode.GetAddress());
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
        }

        return Cycles + extraCycle;
    }
}
