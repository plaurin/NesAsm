using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record SBC : Instruction
{
    public SBC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "SBC", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
