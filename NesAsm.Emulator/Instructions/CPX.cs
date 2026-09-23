using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CPX : Instruction
{
    public CPX(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "CPX", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetC(Cpu.X >= value);
        Cpu.FlagNZ((byte)(Cpu.X - value));
    }
}
