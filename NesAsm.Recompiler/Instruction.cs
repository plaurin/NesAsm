namespace NesAsm.Recompiler;

public record Instruction(int Address, byte Opcode, string Mnemonic, int Bytes, AddressingMode Mode, int? Argument = null)
{
    public static bool IsEndOfSubroutine(string mnemonic) => mnemonic == "JMP" || mnemonic == "RTS" || mnemonic == "RTI";
    public static bool IsJump(string mnemonic) => mnemonic == "JMP" || mnemonic == "JSR";
    public static bool IsBranch(string mnemonic) => mnemonic == "BPL" || mnemonic == "BMI" || mnemonic == "BVC" || mnemonic == "BVS"
        || mnemonic == "BCC" || mnemonic == "BCS" || mnemonic == "BEQ" || mnemonic == "BNE";

    public override string ToString()
    {
        var argument = Mode switch
        {
            AddressingMode.Implicit => string.Empty,
            AddressingMode.Accumulator => "A",
            AddressingMode.Immediate => $"#{Argument:X2}",
            AddressingMode.ZeroPage => $"${Argument:X2}",
            AddressingMode.ZeroPageX => $"${Argument:X2}, X",
            AddressingMode.Absolute => Labels.GetLabelOrMemoryAddress(Argument),
            AddressingMode.AbsoluteX => $"{Labels.GetLabelOrMemoryAddress(Argument)}, X",
            AddressingMode.AbsoluteY => $"{Labels.GetLabelOrMemoryAddress(Argument)}, Y",
            AddressingMode.Relative => Labels.GetLabelOrMemoryAddress(Argument),
            AddressingMode.IndirectIndexed => $"(${Argument:X2}), Y",
            AddressingMode.Indirect => $"({Labels.GetLabelOrMemoryAddress(Argument)})",
            _ => Labels.GetLabelOrMemoryAddress(Argument)
        };

        return $"${Address:X4} [{Opcode:X2}] {Mnemonic} {argument}".Trim();
    }
}
