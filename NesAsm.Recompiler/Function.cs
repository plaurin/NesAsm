namespace NesAsm.Recompiler;

public record Function(ICollection<Instruction> Instructions, IReadOnlyCollection<Jump> Jumps)
{
    public int Address => Instructions.First().Address;
    public int LastInstructionAddress => Instructions.Last().Address;
    public int Size => LastInstructionAddress - Address + Instructions.Last().Bytes;

    public override string ToString()
    {
        return $"- Func at ${Address:X4}";
    }
}