using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BCS : Instruction
{
    public BCS(CPU cpu)
        : base(cpu, 0xB0, "BCS", new Relative(cpu), 2)
    {
    }

    public override void Execute()
    {
        if (Cpu.Carry)
        {
            Cpu.SetPC(AddressMode.GetAddress());
        }
    }
}
