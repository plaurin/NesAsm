using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record INC : Instruction
{
    public INC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "INC", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
