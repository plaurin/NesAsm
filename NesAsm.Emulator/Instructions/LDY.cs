using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record LDY : Instruction
{
    public LDY(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "LDY", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        var res = AddressMode.GetValue();
        Cpu.SetY_NZ(res);

        return Cycles + AddressMode.GetExtraCycle(Cpu.PC);
    }
}
