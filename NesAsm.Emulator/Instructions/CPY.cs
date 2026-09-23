using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CPY : Instruction
{
    public CPY(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "CPY", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetC(Cpu.Y >= value);
        Cpu.FlagNZ((byte)(Cpu.Y - value));
    }
}
