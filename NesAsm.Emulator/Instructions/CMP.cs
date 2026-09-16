using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record CMP : Instruction
{
    public CMP(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "CMP", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetC(Cpu.A >= value);
        Cpu.FlagNZ((byte)(Cpu.A - value));
    }
}
