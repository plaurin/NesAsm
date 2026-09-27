using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record RTI : Instruction
{
    public RTI(CPU cpu)
        : base(cpu, 0x40, "RTI", new Implied(), 6, bytesOverride: 0)
    {
    }

    public override void Execute()
    {
        Cpu.PullFlagsFromStack();

        var lo = Cpu.PopStack();
        var hi = Cpu.PopStack();
        var address = (ushort)(hi * 256 + lo);

        Cpu.SetPC(address);
    }
}
