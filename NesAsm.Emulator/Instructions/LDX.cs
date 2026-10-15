using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record LDX : Instruction
{
    public LDX(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "LDX", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        var res = AddressMode.GetValue();
        Cpu.SetX_NZ(res);

        return Cycles + AddressMode.GetExtraCycle(Cpu.PC);
    }
}
