using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record PLP : Instruction
{
    public PLP(CPU cpu)
        : base(cpu, 0x28, "PLP", new Implied(), 4)
    {
    }

    public override void Execute()
    {
        var value = Cpu.PopStack();
        Cpu.SetN((value & 0x80) == 0x80);
        Cpu.SetO((value & 0x40) == 0x40);
        Cpu.SetD((value & 0x08) == 0x08);
        Cpu.SetI((value & 0x04) == 0x04);
        Cpu.SetZ((value & 0x02) == 0x02);
        Cpu.SetC((value & 0x01) == 0x01);
    }
}
