namespace NesAsm.Recompiler;

public class Parser
{
    public static IReadOnlyCollection<Subroutine> ParsePrgRom(byte[] prgRom, int resetVector, int nmiVector)
    {
        var addressesToParse = new Queue<int>();
        
        addressesToParse.Enqueue(resetVector);
        addressesToParse.Enqueue(nmiVector);
        Labels.AddRomLabel(resetVector, "Reset");
        Labels.AddRomLabel(nmiVector, "NMI");

        // Custom dispatch subroutine addresses to parse
        addressesToParse.Enqueue(0xAEDC);

        var subroutines = new List<Subroutine>();

        while (addressesToParse.Count > 0)
        {
            var address = addressesToParse.Dequeue();
            if (address < 0x8000 || address >= 0x8000 + prgRom.Length)
            {
                Console.WriteLine($"Address ${address:X4} is out of bounds, skipping");
                continue;
            }

            if (subroutines.Any(f => f.Address >= address && f.LastInstructionAddress <= address))
            {
                Console.WriteLine($"Subroutine at ${address:X4} already parsed, skipping");
                continue;
            }

            subroutines.Add(ParseSub(prgRom, addressesToParse, address));
        }

        return subroutines.OrderBy(f => f.Address).ToList();
    }

    private static Subroutine ParseSub(byte[] prgRom, Queue<int> addressesToParse, int address)
    {
        Console.WriteLine($"Parsing sub at ${address:X4}");

        var branchesToParse = new Queue<int>();
        branchesToParse.Enqueue(address);

        var instructions = new List<Instruction>();

        while (branchesToParse.TryDequeue(out address))
        {
            for (int i = 0; i < 500; i++)
            {
                if (instructions.Any(f => f.Address == address)) break; // Already parsed this instruction, exit

                var ins = GetInstruction(prgRom, address);
                instructions.Add(ins);
                address += ins.Bytes;

                //Console.WriteLine(ins.ToString());

                if (Instruction.IsBranch(ins.Mnemonic))
                {
                    //Console.WriteLine($"Found {ins.Mnemonic} to ${ins.Argument!.Value:X4}");
                    branchesToParse.Enqueue(ins.Argument!.Value);
                }

                if (Instruction.IsJump(ins.Mnemonic))
                {
                    Console.WriteLine($"Found {ins.Mnemonic} to ${ins.Argument!.Value:X4}");
                    addressesToParse.Enqueue(ins.Argument!.Value);
                }

                if (Instruction.IsEndOfSubroutine(ins.Mnemonic))
                {
                    Console.WriteLine($"Found {ins.Mnemonic} at ${ins.Address:X4} : end of sub ({instructions.Count} instructions)");
                    break;
                }
            }
        }

        return new Subroutine(instructions.OrderBy(i => i.Address).ToList());
    }

    public static Instruction GetInstruction(byte[] prgRom, int address)
    {
        var romIndex = address - 0x8000;
        var opcode = prgRom[romIndex];

        Instruction Ins(string mnemonic, int bytes, (AddressingMode mode, int? argument) args) => new(address, opcode, mnemonic, bytes, args.mode, args.argument);

        (AddressingMode, int?) Implicit() => (AddressingMode.Implicit, null);
        (AddressingMode, int?) Accumulator() => (AddressingMode.Accumulator, null);
        (AddressingMode, int?) Immediate() => (AddressingMode.Immediate, prgRom[romIndex + 1]);
        (AddressingMode, int?) ZeroPage() => (AddressingMode.ZeroPage, prgRom[romIndex + 1]);
        (AddressingMode, int?) ZeroPageX() => (AddressingMode.ZeroPageX, prgRom[romIndex + 1]);
        (AddressingMode, int?) Absolute() => (AddressingMode.Absolute, prgRom[romIndex + 2] * 256 + prgRom[romIndex + 1]);
        (AddressingMode, int?) AbsoluteX() => (AddressingMode.AbsoluteX, prgRom[romIndex + 2] * 256 + prgRom[romIndex + 1]);
        (AddressingMode, int?) AbsoluteY() => (AddressingMode.AbsoluteY, prgRom[romIndex + 2] * 256 + prgRom[romIndex + 1]);
        (AddressingMode, int?) Relative() => (AddressingMode.Relative, address + 2 + (sbyte)prgRom[romIndex + 1]);
        (AddressingMode, int?) Indirect() => (AddressingMode.Indirect, prgRom[romIndex + 1]);
        (AddressingMode, int?) IndirectIndexed() => (AddressingMode.IndirectIndexed, prgRom[romIndex + 1]);

        return opcode switch
        {
            0x00 => Ins("BRK", 1, Implicit()),
            0x05 => Ins("ORA", 2, ZeroPage()),
            0x08 => Ins("PHP", 1, Implicit()),
            0x09 => Ins("ORA", 2, Immediate()),
            0x0A => Ins("ASL", 1, Accumulator()),

            0x10 => Ins("BPL", 2, Relative()),
            0x18 => Ins("CLC", 1, Implicit()),

            0x20 => Ins("JSR", 3, Absolute()),
            0x29 => Ins("AND", 2, Immediate()),
            0x2A => Ins("ROL", 1, Accumulator()),
            0x2C => Ins("BIT", 3, Absolute()),
            0x2D => Ins("AND", 3, Absolute()),

            0x30 => Ins("BMI", 2, Relative()),
            0x38 => Ins("SEC", 1, Implicit()),
            0x3D => Ins("AND", 3, AbsoluteX()),

            0x40 => Ins("RTI", 1, Implicit()),
            0x45 => Ins("EOR", 2, ZeroPage()),
            0x48 => Ins("PHA", 1, Implicit()),
            0x4A => Ins("LSR", 1, Accumulator()),
            0x4C => Ins("JMP", 3, Absolute()),

            0x60 => Ins("RTS", 1, Implicit()),
            0x65 => Ins("ADC", 2, ZeroPage()),
            0x68 => Ins("PLA", 1, Implicit()),
            0x69 => Ins("ADC", 2, Immediate()),
            0x6C => Ins("JMP", 3, IndirectIndexed()), // Based on memory!! we need to emulate the memory to get the correct address
            0x6D => Ins("ADC", 3, Absolute()),

            0x78 => Ins("SEI", 1, Implicit()),
            0x79 => Ins("ADC", 3, AbsoluteY()),
            0x7D => Ins("ADC", 3, AbsoluteX()),
            0x7E => Ins("ROR", 3, AbsoluteX()),

            0x84 => Ins("STY", 2, ZeroPage()),
            0x85 => Ins("STA", 2, ZeroPage()),
            0x86 => Ins("STX", 2, ZeroPage()),
            0x88 => Ins("DEY", 1, Implicit()),
            0X8A => Ins("TXA", 1, Implicit()),
            0x8C => Ins("STY", 3, Absolute()),
            0x8D => Ins("STA", 3, Absolute()),
            0x8E => Ins("STX", 3, Absolute()),

            0x90 => Ins("BCC", 2, Relative()),
            0x91 => Ins("STA", 2, IndirectIndexed()),
            0x98 => Ins("TYA", 1, Implicit()),
            0x99 => Ins("STA", 3, AbsoluteY()),
            0x9A => Ins("TXS", 1, Implicit()),
            0x9D => Ins("STA", 3, AbsoluteX()),

            0xA0 => Ins("LDY", 2, Immediate()),
            0xA2 => Ins("LDX", 2, Immediate()),
            0xA4 => Ins("LDY", 2, ZeroPage()),
            0xA5 => Ins("LDA", 2, ZeroPage()),
            0xA6 => Ins("LDX", 2, ZeroPage()),
            0xA8 => Ins("TAY", 1, Implicit()),
            0xA9 => Ins("LDA", 2, Immediate()),
            0xAA => Ins("TAX", 1, Implicit()),
            0xAC => Ins("LDY", 3, Absolute()),
            0xAD => Ins("LDA", 3, Absolute()),
            0xAE => Ins("LDX", 3, Absolute()),

            0xB0 => Ins("BCS", 2, Relative()),
            0xB1 => Ins("LDA", 2, IndirectIndexed()),
            0xB5 => Ins("LDA", 2, ZeroPageX()),
            0xB9 => Ins("LDA", 3, AbsoluteY()),
            0xBD => Ins("LDA", 3, AbsoluteX()),
            0xBE => Ins("LDX", 3, AbsoluteY()),

            0xC0 => Ins("CPY", 2, Immediate()),
            0xC5 => Ins("CMP", 2, ZeroPage()),
            0xC8 => Ins("INY", 1, Implicit()),
            0xC9 => Ins("CMP", 2, Immediate()),
            0xCA => Ins("DEX", 1, Implicit()),
            0xCE => Ins("DEC", 3, Absolute()),

            0xD0 => Ins("BNE", 2, Relative()),
            0xD8 => Ins("CLD", 1, Implicit()),
            0xD9 => Ins("CMP", 3, AbsoluteY()),
            0xDE => Ins("DEC", 3, AbsoluteX()),

            0xE0 => Ins("CPX", 2, Immediate()),
            0xE6 => Ins("INC", 2, ZeroPage()),
            0xE8 => Ins("INX", 1, Implicit()),
            0xE9 => Ins("SBC", 2, Immediate()),
            0xED => Ins("SBC", 3, Absolute()),
            0xEE => Ins("INC", 3, Absolute()),

            0xF0 => Ins("BEQ", 2, Relative()),
            0xF5 => Ins("SBC", 2, ZeroPageX()),
            0xF9 => Ins("SBC", 3, AbsoluteY()),

            _ => Ins($"Unknown Opcode: {opcode:X2}", 1, (AddressingMode.Implicit, null))
        };
    }
}

public record Jump(int Address, int TargetAddress);

public record Branch(int Address, int TargetAddress);
