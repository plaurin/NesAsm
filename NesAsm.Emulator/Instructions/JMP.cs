using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record JMP : Instruction
{
    public JMP(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "JMP", addressMode, cycles, bytesOverride: 0)
    {
    }

    public override void Execute()
    {
        var targetPC = AddressMode.GetAddress();
        Cpu.SetPC(targetPC);
    }
}
