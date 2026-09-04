using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var (gameName, romPath, outputPath) = ParseArguments(args);

        Console.WriteLine($"Parsing game {gameName} at {Path.GetFileNameWithoutExtension(romPath)}");
        Console.WriteLine($"Output path: {outputPath}");

        Directory.CreateDirectory(outputPath);

        var rom = File.ReadAllBytes(romPath);

        if (!(rom[0] == 0x4E && rom[1] == 0x45 && rom[2] == 0x53 && rom[3] == 0x1A))
        {
            Console.WriteLine("NES ROM not detected!");
            return;
        }

        Console.WriteLine("NES ROM detected.");

        var prgSize = rom[4];
        var chrSize = rom[5];
        Console.WriteLine($"PRG Size: {prgSize} * 16 * 1024 = {prgSize * 16 * 1024}");
        Console.WriteLine($"CHR Size: {chrSize} * 8 * 1024 = {chrSize * 8 * 1024}");

        // Flag 6
        var flag6 = rom[6];
        var mirroring = (flag6 & 0x01) != 0 ? "Vertical" : "Horizontal";
        var batteryBacked = (flag6 & 0x02) != 0 ? "Yes" : "No";
        var trainer = (flag6 & 0x04) != 0 ? "Yes" : "No";
        var fourScreen = (flag6 & 0x08) != 0 ? "Yes" : "No";
        var mapperLow = (flag6 & 0xF0) >> 4;

        Console.WriteLine($"Mirroring: {mirroring}");
        Console.WriteLine($"Battery Backed: {batteryBacked}");
        Console.WriteLine($"Trainer: {trainer}");
        Console.WriteLine($"Four Screen: {fourScreen}");
        Console.WriteLine($"Mapper (Low): {mapperLow}");

        var prgRom = rom.Skip(16).Take(prgSize * 16 * 1024).ToArray();
        var chrRom = rom.Skip(16 + prgRom.Length).Take(chrSize * 8 * 1024).ToArray();

        var mainFunc = ParseFunc(prgRom, 0x8000);
        var functions = new List<Function> { mainFunc };

        foreach (var ins in mainFunc.Instructions)
        {
            if (ins.Mnemonic == "JSR")
            {
                functions.Add(ParseFunc(prgRom, ins.Argument!.Value));
            }
        }

        var sb = new StringBuilder();
        foreach (var func in functions)
        {
            sb.AppendLine(func.ToString());
            foreach (var instruction in func.Instructions)
            {
                sb.AppendLine(instruction.ToString());
            }
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(outputPath, "instructions.txt"), sb.ToString());
    }

    private static (string gameName, string romPath, string outputPath) ParseArguments(string[] args)
    {
        const string projectFolder = "NesAsm.Recompiler";
        var cur = Directory.GetCurrentDirectory();
        var ind = cur.IndexOf(projectFolder) + projectFolder.Length;
        var argFilePath = Path.Combine(cur[..ind], "args");

        if (args.Length == 0)
        {
            if (!File.Exists(argFilePath))
            {
                throw new FileNotFoundException($"Argument file not found: {argFilePath}");
            }
            var argLines = File.ReadAllLines(argFilePath);
            if (argLines.Length != 3)
            {
                throw new ArgumentException("Invalid number of arguments");
            }
            return (argLines[0], argLines[1], argLines[2]);
        }
        else if (args.Length != 3)
        {
            throw new ArgumentException("Invalid number of arguments.");
        }

        var gameName = args[0];
        var romPath = args[1];
        var outputPath = args[2];

        File.WriteAllLines(argFilePath, [gameName, romPath, outputPath]);

        return (gameName, romPath, outputPath);
    }

    private static Function ParseFunc(byte[] prgRom, int address)
    {
        Console.WriteLine($"Parsing func at ${address:X4}");

        var instructions = new List<Instruction>();
        for (int i = 0; i < 100; i++)
        {
            var ins = GetInstruction(prgRom, address);
            instructions.Add(ins);

            Console.WriteLine(ins.ToString());
            address += ins.Bytes;

            if (ins.Mnemonic == "JMP" || ins.Mnemonic == "RTS" || ins.Mnemonic == "RTI")
            {
                Console.WriteLine($"Found {ins.Mnemonic} at ${ins.Address:X4} : end of func ({instructions.Count} instructions)");
                break;
            }
        }

        return new Function(instructions);
    }

    public static Instruction GetInstruction(byte[] prgRom, int address)
    {
        var romIndex = address - 0x8000;
        var opcode = prgRom[romIndex];

        Instruction Ins(string mnemonic, int bytes, (AddressingMode mode, int? argument) args) => new(address, opcode, mnemonic, bytes, args.mode, args.argument);

        (AddressingMode, int?) Implicit() => (AddressingMode.Implicit, null);
        (AddressingMode, int?) Immediate() => (AddressingMode.Immediate, prgRom[romIndex + 1]);
        (AddressingMode, int?) ZeroPage() => (AddressingMode.ZeroPage, prgRom[romIndex + 1]);
        (AddressingMode, int?) Absolute() => (AddressingMode.Absolute, prgRom[romIndex + 2] * 256 + prgRom[romIndex + 1]);
        (AddressingMode, int?) AbsoluteX() => (AddressingMode.AbsoluteX, prgRom[romIndex + 2] * 256 + prgRom[romIndex + 1]);
        (AddressingMode, int?) AbsoluteY() => (AddressingMode.AbsoluteY, prgRom[romIndex + 2] * 256 + prgRom[romIndex + 1]);
        (AddressingMode, int?) Relative() => (AddressingMode.Relative, address + 2 + (sbyte)prgRom[romIndex + 1]);
        (AddressingMode, int?) IndirectIndexed() => (AddressingMode.IndirectIndexed, prgRom[romIndex + 1]);

        return opcode switch
        {
            0x00 => Ins("BRK", 1, Implicit()),
            0x08 => Ins("PHP", 1, Implicit()),
            0x09 => Ins("ORA", 2, Immediate()),

            0x10 => Ins("BPL", 2, Relative()),

            0x20 => Ins("JSR", 3, Absolute()),
            0x29 => Ins("AND", 2, Immediate()),
            0x2C => Ins("BIT", 3, Absolute()),

            0x4C => Ins("JMP", 3, Absolute()),

            0x60 => Ins("RTS", 1, Implicit()),

            0x78 => Ins("SEI", 1, Implicit()),

            0x85 => Ins("STA", 2, ZeroPage()),
            0x86 => Ins("STX", 2, ZeroPage()),
            0x88 => Ins("DEY", 1, Implicit()),
            0X8A => Ins("TXA", 1, Implicit()),
            0x8D => Ins("STA", 3, Absolute()),

            0x91 => Ins("STA", 2, IndirectIndexed()),
            0x9A => Ins("TXS", 1, Implicit()),
            0x99 => Ins("STA", 3, AbsoluteY()),

            0xA0 => Ins("LDY", 2, Immediate()),
            0xA2 => Ins("LDX", 2, Immediate()),
            0xA9 => Ins("LDA", 2, Immediate()),
            0xAD => Ins("LDA", 3, Absolute()),

            0xB0 => Ins("BCS", 2, Relative()),
            0xBD => Ins("LDA", 3, AbsoluteX()),

            0xC0 => Ins("CPY", 2, Immediate()),
            0xC8 => Ins("INY", 1, Implicit()),
            0xC9 => Ins("CMP", 2, Immediate()),
            0xCA => Ins("DEX", 1, Implicit()),

            0xD0 => Ins("BNE", 2, Relative()),
            0xD8 => Ins("CLD", 1, Implicit()),

            0xE0 => Ins("CPX", 2, Immediate()),
            0xEE => Ins("INC", 3, Absolute()),

            _ => Ins($"Unknown Opcode: {opcode:X2}", 1, (AddressingMode.Implicit, null))
        };
    }

    public enum AddressingMode
    {
        Implicit,
        Immediate,
        ZeroPage,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Relative,
        IndirectIndexed,
    }

    public record Instruction(int Address, byte Opcode, string Mnemonic, int Bytes, AddressingMode Mode, int? Argument = null)
    {
        public override string ToString()
        {
            return Mode switch
            {
                AddressingMode.Implicit => $"${Address:X4}: {Mnemonic}",
                AddressingMode.Immediate => $"${Address:X4}: {Mnemonic} #${Argument:X2}",
                AddressingMode.ZeroPage => $"${Address:X4}: {Mnemonic} ${Argument:X2}",
                AddressingMode.Absolute => $"${Address:X4}: {Mnemonic} ${Argument:X4}",
                AddressingMode.AbsoluteX => $"${Address:X4}: {Mnemonic} ${Argument:X4}, X",
                AddressingMode.AbsoluteY => $"${Address:X4}: {Mnemonic} ${Argument:X4}, Y",
                AddressingMode.Relative => $"${Address:X4}: {Mnemonic} ${Argument:X4}",
                AddressingMode.IndirectIndexed => $"${Address:X4}: {Mnemonic} (${Argument:X2}), Y",
                _ => $"${Address:X4}: {Mnemonic} {Argument:X4}"
            };
        }
    }

    public record Function(ICollection<Instruction> Instructions)
    {
        public override string ToString()
        {
            return $"- Func at ${Instructions.First().Address:X4}";
        }
    }
}