using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record STA : Instruction
{
    public STA(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "STA", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        AddressMode.SetValue(Cpu.A);
    }
}
