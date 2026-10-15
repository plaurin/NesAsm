using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record STY : Instruction
{
    public STY(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "STY", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        AddressMode.SetValue(Cpu.Y);

        return Cycles;
    }
}
