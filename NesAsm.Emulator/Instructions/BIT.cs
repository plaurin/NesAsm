using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BIT : Instruction
{
    public BIT(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "BIT", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetZ((Cpu.A & value) == 0);
        Cpu.SetO((value & 0b_0100_0000) == 0b_0100_0000);
        Cpu.SetN((value & 0b_1000_0000) == 0b_1000_0000);
    }
}
