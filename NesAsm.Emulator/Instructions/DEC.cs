using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record DEC : Instruction
{
    public DEC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "DEC", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
