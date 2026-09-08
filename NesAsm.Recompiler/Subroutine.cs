namespace NesAsm.Recompiler;

public record Subroutine(ICollection<Instruction> Instructions)
{
    public int Address => Instructions.First().Address;
    public int LastInstructionAddress => Instructions.Last().Address;
    public int Size => LastInstructionAddress - Address + Instructions.Last().Bytes;

    public IEnumerable<Jump> Jumps => Instructions
        .Where(i => Instruction.IsJump(i.Mnemonic) || Instruction.IsJumpToSubroutine(i.Mnemonic))
        .Select(i => new Jump(i.Address, i.Argument!.Value));

    public IEnumerable<Branch> Branches => Instructions
        .Where(i => Instruction.IsBranch(i.Mnemonic))
        .Select(i => new Branch(i.Address, i.Argument!.Value));

    public bool Returns => Instructions
        .Any(i => Instruction.IsReturnInstruction(i.Mnemonic));

    public IEnumerable<Instruction> DynamicDispathes => Instructions
        .Where(i => Instruction.IsDynamicDispatch(i.Opcode));

    public bool EndsWithJSR => Instruction.IsJumpToSubroutine(Instructions.Last().Mnemonic);

    public override string ToString()
    {
        var label = Labels.GetLabel(Address);
        label = !string.IsNullOrWhiteSpace(label) ? $"{label} at " : string.Empty;
        return $"Sub {label}${Address:X4} to ${LastInstructionAddress:X4} (Instructions: {Instructions.Count}, Size: {Size})";
    }
}