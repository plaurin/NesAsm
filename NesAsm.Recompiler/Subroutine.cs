namespace NesAsm.Recompiler;

public record Subroutine(ICollection<Instruction> Instructions)
{
    public int Address => Instructions.First().Address;
    public int LastInstructionAddress => Instructions.Last().Address;
    public int Size => LastInstructionAddress - Address + Instructions.Last().Bytes;

    public IEnumerable<Jump> Jumps => Instructions
        .Where(i => Instruction.IsJump(i.Mnemonic))
        .Select(i => new Jump(i.Address, i.Argument!.Value));

    public IEnumerable<Branch> Branches => Instructions
        .Where(i => Instruction.IsBranch(i.Mnemonic))
        .Select(i => new Branch(i.Address, i.Argument!.Value));

    public override string ToString()
    {
        return $"- Sub {Labels.GetRomLabel(Address)} at ${Address:X4}";
    }
}