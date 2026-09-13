using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ORA : Instruction
{
    public ORA(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ORA", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
