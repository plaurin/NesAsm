using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public record EOR : Instruction
{
    public EOR(CPU cpu, byte opcode, AddressMode addressMode, int cycles)
        : base(cpu, opcode, "EOR", addressMode, cycles)
    {
    }

    public override int Execute()
    {
        var value = AddressMode.GetValue();
        Cpu.SetA_NZ((byte)(Cpu.A ^ value));

        return Cycles + AddressMode.GetExtraCycle(Cpu.PC);
    }
}
