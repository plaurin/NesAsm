using NesAsm.Emulator.AddressModes;

namespace NesAsm.Emulator.Instructions;

public abstract record Instruction(CPU Cpu, byte Opcode, string Mnemonic, AddressMode AddressMode, int Cycles)
{
    public void Run()
    {
        Execute();
    }

    protected abstract void Execute();
}
