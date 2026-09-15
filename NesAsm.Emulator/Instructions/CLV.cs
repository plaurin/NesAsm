using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CLV : Instruction
{
    public CLV(CPU cpu)
        : base(cpu, 0xB8, "CLV", new Implied(), 2)
    {
    }

    public override void Execute() => Cpu.SetO(false);
}
