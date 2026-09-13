using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record LDA : Instruction
{
    public LDA(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "LDA", addressMode, cycles)
    {
    }

    protected override void Execute()
    {
        var res = AddressMode.GetValue();
        Cpu.SetA_NZ(res);
        //_cycles += cycles + addressing.ExtraCycle;
    }
}
