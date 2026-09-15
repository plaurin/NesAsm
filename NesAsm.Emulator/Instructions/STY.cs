using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record STY : Instruction
{
    public STY(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "STY", addressMode, cycles)
    {
    }

    public override void Execute()
    {
        AddressMode.SetValue(Cpu.Y);
        //_cycles += cycles + addressing.ExtraCycle;
    }
}
