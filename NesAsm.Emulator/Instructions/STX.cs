using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record STX : Instruction
{
    public STX(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "STX", addressMode, cycles)
    {
    }

    protected override void Execute()
    {
        AddressMode.SetValue(Cpu.X);
        //_cycles += cycles + addressing.ExtraCycle;
    }
}
