using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record EOR : Instruction
{
    public EOR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "EOR", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
