using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record RTS : Instruction
{
    public RTS(CPU cpu)
        : base(cpu, 0x60, "RTS", new Implied(), 6, bytesOverride: 0)
    {
    }

    public override void Execute()
    {
        var hi = Cpu.PopStack();
        var lo = Cpu.PopStack();
        var address = (ushort)(hi * 256 + lo);

        Cpu.SetPC((ushort)(address + 1));
    }
}
