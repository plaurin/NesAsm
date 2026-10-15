using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record SED : Instruction
{
    public SED(CPU cpu)
        : base(cpu, 0xF8, "SED", new Implied(), 2)
    {
    }

    public override int Execute()
    {
        Cpu.SetD(true);

        return Cycles;
    }
}
