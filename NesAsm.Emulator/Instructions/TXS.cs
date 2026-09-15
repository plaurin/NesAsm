using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TXS : Instruction
{
    public TXS(CPU cpu)
        : base(cpu, 0x9A, "TXS", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetSP(Cpu.X);
}
