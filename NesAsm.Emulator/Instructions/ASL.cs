using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ASL : Instruction
{
    public ASL(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ASL", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetC((value & 0b_1000_0000) == 0b_1000_0000);
        value <<= 1;
        AddressMode.SetValue(value);
    }
}
