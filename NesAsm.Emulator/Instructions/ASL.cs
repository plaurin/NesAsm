using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ASL : Instruction
{
    public ASL(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ASL", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
