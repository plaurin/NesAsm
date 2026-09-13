using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ROR : Instruction
{
    public ROR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ROR", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
