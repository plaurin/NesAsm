using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record DEX : Instruction
{
    public DEX(CPU cpu)
        : base(cpu, 0xCA, "DEX", new Implied(), 2)
    {
    }

    public override void Execute()
    {
        Cpu.SetX_NZ((byte)(Cpu.X - 1));
    }
}
