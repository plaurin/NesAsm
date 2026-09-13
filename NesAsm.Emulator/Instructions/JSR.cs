using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record JSR : Instruction
{
    public JSR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "JSR", addressMode, cycles)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}
