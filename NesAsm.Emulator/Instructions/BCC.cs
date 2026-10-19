using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BCC : Instruction
{
    public BCC(CPU cpu)
        : base(cpu, 0x90, "BCC", new Relative(cpu), 2)
    {
    }

    public override int Execute()
    {
        var extraCycle = 0;

        if (!Cpu.Carry)
        {
            extraCycle = 1 + AddressMode.GetExtraCycle(Cpu.PC);
            Cpu.SetPC(AddressMode.GetAddress());
        }

        return Cycles + extraCycle;
    }
}
