using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record INY : Instruction
{
    public INY(CPU cpu)
        : base(cpu, 0xC8, "INY", new Implied(), 2)
    {
    }

    public override void Execute()
    {
        Cpu.SetY_NZ((byte)(Cpu.Y + 1));
    }
}
