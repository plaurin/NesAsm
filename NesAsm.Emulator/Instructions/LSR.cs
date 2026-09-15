using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record LSR : Instruction
{
    public LSR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "LSR", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
