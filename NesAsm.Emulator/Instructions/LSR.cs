using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record LSR : Instruction
{
    public LSR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "LSR", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetC((value & 0b_0000_0001) == 0b_0000_0001);
        value >>= 1;
        AddressMode.SetValue(value);
    }
}
