using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CPY : Instruction
{
    public CPY(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "CPY", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
