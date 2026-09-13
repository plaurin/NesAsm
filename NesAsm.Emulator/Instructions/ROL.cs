using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ROL : Instruction
{
    public ROL(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ROL", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
