using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ROL : Instruction
{
    public ROL(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ROL", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        var carry = (byte)(Cpu.Carry ? 1 : 0);

        Cpu.SetC((value & 0b_1000_0000) == 0b_1000_0000);
        value <<= 1;
        value += carry;

        AddressMode.SetValue(value);
        Cpu.FlagNZ(value);
    }
}
