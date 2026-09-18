namespace NesAsm.Recompiler;

public record Subroutine(ICollection<Instruction> Instructions)
{
    public ushort Address => Instructions.First().Address;
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

    public string Label => Labels.GetLabel(Address);
    public string LabelOrAddress => string.IsNullOrEmpty(Label) ? $"{Address:X4}" : Label;

    public override string ToString()
    {
        var label = !string.IsNullOrWhiteSpace(Label) ? $"{Label} at " : string.Empty;
        var flags = GetFlags();
        return $"Sub {label}${Address:X4} to ${LastInstructionAddress:X4} (Instructions: {Instructions.Count}, Size: {Size})  {flags}";
    }

    public IEnumerable<MemoryAccessRecord> GetDirectAccess() => GetMemoryAccess().Where(m => m.IsDirectAccess);

    public IEnumerable<MemoryAccessRecord> GetIndirectAccess() => GetMemoryAccess().Where(m => !m.IsDirectAccess);

    public IEnumerable<MemoryAccessRecord> GetDynamicDispatch() => Instructions.Where(i => Instruction.IsDynamicDispatch(i.Opcode)).Select(i => i.GetMemoryAccess(this)!);

    public IEnumerable<MemoryAccessRecord> GetMemoryAccess() => Instructions.Select(i => i.GetMemoryAccess(this)).Where(m => m != null).Select(m => m!);

    private string GetFlags()
    {
        var flags = new List<string>();
        var memoryAccess = GetMemoryAccess();

        if (memoryAccess.Any(m => m.IsDynamicDispatch)) flags.Add("[DynamicDispatch]");
        if (memoryAccess.Any(m => m.IsZeroPageAccess)) flags.Add("[ZeroPage]");
        if (memoryAccess.Any(m => m.IsStackPageAccess)) flags.Add("[StackPage]");
        if (memoryAccess.Any(m => m.IsOAMPageAccess)) flags.Add("[OAMPage]");
        if (memoryAccess.Any(m => m.IsOtherRAMPageAccess)) flags.Add("[OtherRAMPage]");
        if (memoryAccess.Any(m => m.IsPPURegisterAccess)) flags.Add("[PPU]");
        if (memoryAccess.Any(m => m.IsAPURegisterAccess)) flags.Add("[APU]");
        if (memoryAccess.Any(m => m.IsOAMDataAccess)) flags.Add("[OAMData]");
        if (memoryAccess.Any(m => m.IsJoypadAccess)) flags.Add("[Joypad]");
        if (memoryAccess.Any(m => m.IsWorkRAMAccess)) flags.Add("[WorkRAM]");
        
        return string.Join(" ", flags);
    }
}