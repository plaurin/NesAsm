using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record JSR : Instruction
{
    public JSR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "JSR", addressMode, cycles, bytesOverride: 0)
    {
    }

    public override void Execute()
    {
        var targetPC = AddressMode.GetAddress();
        Cpu.PushStack((ushort)(Cpu.PC + 2));

        Cpu.SetPC(targetPC);
    }
}
