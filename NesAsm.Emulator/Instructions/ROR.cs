using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ROR : Instruction
{
    public ROR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ROR", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        var carry = (byte)(Cpu.Carry ? 0x80 : 0);

        Cpu.SetC((value & 0b_0000_0001) == 0b_0000_0001);
        value >>= 1;
        value += carry;

        AddressMode.SetValue(value);
        Cpu.FlagNZ(value);
    }
}
