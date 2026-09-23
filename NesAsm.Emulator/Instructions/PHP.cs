using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record PHP : Instruction
{
    public PHP(CPU cpu)
        : base(cpu, 0x08, "PHP", new Implied(), 3)
    {
    }

    public override void Execute()
    {
        var value =
            (Cpu.Negative ? 0x80 : 0) +
            (Cpu.Overflow ? 0x40 : 0) +
            0x20 +
            0x10 +
            (Cpu.Decimal ? 0x08 : 0) +
            (Cpu.Interrupt ? 0x04 : 0) +
            (Cpu.Zero ? 0x02 : 0) +
            (Cpu.Carry ? 0x01 : 0);

        Cpu.PushStack((byte)value);
    }
}
