using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record BMI : Instruction
{
    public BMI(CPU cpu)
        : base(cpu, 0x30, "BMI", new Relative(cpu), 2)
    {
    }

    protected override void Execute() => throw new NotImplementedException();
}




