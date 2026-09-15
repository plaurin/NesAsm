using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BIT : Instruction
{
    public BIT(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "BIT", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
