using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record NOP : Instruction
{
    public NOP(CPU cpu)
        : base(cpu, 0xEA, "NOP", new Implied(), 2)
    {
    }

    public override void Execute() { } // No Op
}

