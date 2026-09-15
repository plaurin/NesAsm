using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CPX : Instruction
{
    public CPX(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "CPX", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
