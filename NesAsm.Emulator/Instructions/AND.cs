using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record AND : Instruction
{
    public AND(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "AND", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
