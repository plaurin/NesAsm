using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record TXA : Instruction
{
    public TXA(CPU cpu)
        : base(cpu, 0x8A, "TXA", new Implied(), 2)
    {
    }

    protected override void Execute()
    {
        Cpu.SetA_NZ(Cpu.X);
    }
}
