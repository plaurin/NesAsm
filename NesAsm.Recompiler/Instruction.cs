namespace NesAsm.Recompiler;

public record Instruction(ushort Address, byte Opcode, string Mnemonic, int Bytes, AddressingMode Mode, ushort? Argument = null)
{
    public byte ByteArgument => (byte)Argument!;
    public ushort UShortArgument => (ushort)Argument!;

    public static bool IsEndOfSubroutine(string mnemonic) => mnemonic == "JMP" || mnemonic == "RTS" || mnemonic == "RTI" || mnemonic == "JSR";

    public static bool IsBranch(string mnemonic) => mnemonic == "BPL" || mnemonic == "BMI" || mnemonic == "BVC" || mnemonic == "BVS"
        || mnemonic == "BCC" || mnemonic == "BCS" || mnemonic == "BEQ" || mnemonic == "BNE";

    public static bool IsReturnInstruction(string mnemonic) => mnemonic == "RTS" || mnemonic == "RTI";

    public static bool IsDynamicDispatch(byte opcode) => opcode == 0x6C;

    public static bool IsJump(string mnemonic) => mnemonic == "JMP";

    public static bool IsJumpToSubroutine(string mnemonic) => mnemonic == "JSR";

    public static bool IsDirectMemoryAccess(AddressingMode mode) => mode == AddressingMode.ZeroPage || mode == AddressingMode.Absolute;
    public static bool IsIndirectMemoryAccess(AddressingMode mode) => mode == AddressingMode.ZeroPageX || mode == AddressingMode.AbsoluteX
        || mode == AddressingMode.AbsoluteY || mode == AddressingMode.IndirectIndexed;

    public static bool IsMemoryRead(string mnemonic) => mnemonic == "LDA" || mnemonic == "LDX" || mnemonic == "LDY" || mnemonic == "ADC" || mnemonic == "AND"
        || mnemonic == "ASL" || mnemonic == "BIT" || mnemonic == "CMP" || mnemonic == "CPX" || mnemonic == "CPY" || mnemonic == "DEC" || mnemonic == "DEX"
        || mnemonic == "DEY" || mnemonic == "EOR" || mnemonic == "INC" || mnemonic == "INX" || mnemonic == "INY" || mnemonic == "LSR" || mnemonic == "ORA"
        || mnemonic == "ROL" || mnemonic == "ROR" || mnemonic == "SBC";

    public static bool IsMemoryWrite(string mnemonic) => mnemonic == "STA" || mnemonic == "STX" || mnemonic == "STY" || mnemonic == "DEC" || mnemonic == "INC"
        || mnemonic == "LSR" || mnemonic == "ASL" || mnemonic == "ROL" || mnemonic == "ROR";

    public override string ToString()
    {
        var argument = Mode switch
        {
            AddressingMode.Implicit => string.Empty,
            AddressingMode.Accumulator => "A",
            AddressingMode.Immediate => $"#${Argument:X2}",
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
        var invalid = IsInvalid ? "*Invalid*" : "";

        return $"${Address:X4} [{Opcode:X2}] {Mnemonic} {argument} {invalid}".Trim();
    }

    public MemoryAccessRecord? GetMemoryAccess(Subroutine subroutine)
    {
        bool? isDirectAccess = null;

        if (IsDirectMemoryAccess(Mode))
            isDirectAccess = true;
        else if (IsIndirectMemoryAccess(Mode))
            isDirectAccess = false;
        else if (IsDynamicDispatch(Opcode))
            isDirectAccess = true;

        if (isDirectAccess.HasValue)
        {
            return new MemoryAccessRecord(subroutine, this,
                RomAddress: Address,
                TargetAddress: UShortArgument,
                IsRead: IsMemoryRead(Mnemonic),
                IsWrite: IsMemoryWrite(Mnemonic),
                IsJump: IsJump(Mnemonic) || IsJumpToSubroutine(Mnemonic),
                IsBranch: IsBranch(Mnemonic),
                IsDirectAccess: isDirectAccess.Value);
        }

        return null;
    }

    public bool IsInvalid
    {
        get
        {
            if (Mnemonic.StartsWith("Unknown")) return true;
            if (Mnemonic.StartsWith("ST") && UShortArgument >= 0x8000) return true;
            if (Mnemonic == "JSR" && UShortArgument <= 0x6000) return true;

            return false;
        }
    }
}
