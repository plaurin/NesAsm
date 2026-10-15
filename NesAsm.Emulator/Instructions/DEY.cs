using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record DEY : Instruction
{
    public DEY(CPU cpu)
        : base(cpu, 0x88, "DEY", new Implied(), 2)
    {
    }

    public override int Execute()
    {
        Cpu.SetY_NZ((byte)(Cpu.Y - 1));

        return Cycles;
    }
}
