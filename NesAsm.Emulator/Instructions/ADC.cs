using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record ADC : Instruction
{
    public ADC(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "ADC", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        var value = AddressMode.GetValue();
        var result = Cpu.A + value + (Cpu.Carry ? 1 : 0);
        Cpu.SetC(result > 0xFF);
        Cpu.SetO(((result ^ Cpu.A) & ((byte)result ^ value) & 0x80) != 0);
        Cpu.SetA_NZ((byte)result);

        return Cycles + AddressMode.GetExtraCycle(Cpu.PC);
    }
}
