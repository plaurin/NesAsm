using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BRK : Instruction
{
    public BRK(CPU cpu)
        : base(cpu, 0x00, "BRK", new Implied(), 7, bytesOverride: 2)
    {
    }

    public override int Execute()
    {
        Cpu.PushStack(Cpu.PC);
        Cpu.PushFlagsOnStack();

        Cpu.SetPC(Cpu.Memory.Cart.IrqAddress);
        
        return Cycles;
    }
}
