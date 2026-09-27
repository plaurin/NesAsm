using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record SBC : Instruction
{
    public SBC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "SBC", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        var value = AddressMode.GetValue() ^ 0xFF;
        var result = Cpu.A + value + (Cpu.Carry ? 1 : 0);
        Cpu.SetC(result > 0xFF);
        Cpu.SetO(((Cpu.A ^ result) & (value ^ result) & 0x80) != 0);
        Cpu.SetA_NZ((byte)result);
    }
}
