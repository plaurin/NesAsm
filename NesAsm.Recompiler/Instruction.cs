namespace NesAsm.Recompiler;

public record Instruction(int Address, byte Opcode, string Mnemonic, int Bytes, AddressingMode Mode, int? Argument = null)
{
    public override string ToString()
    {

        return Mode switch
        {
            AddressingMode.Implicit => $"${Address:X4}: {Mnemonic}",
            AddressingMode.Accumulator => $"${Address:X4}: {Mnemonic} A",
            AddressingMode.Immediate => $"${Address:X4}: {Mnemonic} #${Argument:X2}",
            AddressingMode.ZeroPage => $"${Address:X4}: {Mnemonic} ${Argument:X2}",
            AddressingMode.ZeroPageX => $"${Address:X4}: {Mnemonic} ${Argument:X2}, X",
            AddressingMode.Absolute => $"${Address:X4}: {Mnemonic} {Labels.GetLabelOrMemoryAddress(Argument)}",
            AddressingMode.AbsoluteX => $"${Address:X4}: {Mnemonic} {Labels.GetLabelOrMemoryAddress(Argument)}, X",
            AddressingMode.AbsoluteY => $"${Address:X4}: {Mnemonic} {Labels.GetLabelOrMemoryAddress(Argument)}, Y",
            AddressingMode.Relative => $"${Address:X4}: {Mnemonic} {Labels.GetLabelOrMemoryAddress(Argument)}",
            AddressingMode.IndirectIndexed => $"${Address:X4}: {Mnemonic} (${Argument:X2}), Y",
            AddressingMode.Indirect => $"${Address:X4}: {Mnemonic} ({Labels.GetLabelOrMemoryAddress(Argument)})",
            _ => $"${Address:X4}: {Mnemonic} {Labels.GetLabelOrMemoryAddress(Argument)}"
        };
    }
}
