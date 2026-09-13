using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record JMP : Instruction
{
    public JMP(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "JMP", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
