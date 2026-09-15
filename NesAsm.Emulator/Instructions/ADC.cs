using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ADC : Instruction
{
    public ADC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ADC", addressMode, cycles)
    {
    }

    public override void Execute() => throw new NotImplementedException();
}
