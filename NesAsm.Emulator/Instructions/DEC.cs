using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record DEC : Instruction
{
    public DEC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "DEC", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        var value = AddressMode.GetValue();
        var result = (byte)(value - 1);
        AddressMode.SetValue(result);
        Cpu.FlagNZ(result);

        return Cycles;
    }
}
