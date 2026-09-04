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

        var instructions = new List<Instruction>();
        var index = 0;
        for (int i = 0; i < 20; i++)
        {
            var ins = GetInstruction(prgRom, index);
            instructions.Add(ins);

            Console.WriteLine($"${ins.Address:X4}: {ins.Mnemonic} {ins.Arguments}");
            index += ins.Bytes;
        }

        var sb = new StringBuilder();
        foreach (var instruction in instructions)
        {
            sb.AppendLine($"${instruction.Address:X4}: {instruction.Mnemonic} {instruction.Arguments}");
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

    public static Instruction GetInstruction(byte[] prgRom, int index)
    {
        var opcode = prgRom[index];

        Instruction Ins(string mnemonic, int bytes, string? arguments = null) => new(0x8000 + index, opcode, mnemonic, bytes, arguments);

        string? Implicit() => null;
        string Immediate() => $"#${prgRom[index + 1]:X2}";
        string Absolute() => $"${prgRom[index + 2]:X2}{prgRom[index + 1]:X2}";
        string AbsoluteX() => $"${prgRom[index + 2]:X2}{prgRom[index + 1]:X2}";
        string Relative() => $"${0x8000 + index + 2 + (sbyte)prgRom[index + 1]:X4}";

        return opcode switch
        {
            0x00 => Ins("BRK", 1, Implicit()),
            //0x01 => Ins("ORA (Indirect,X)", 2),
            //0x05 => Ins("ORA Zero Page", 2),
            //0x06 => Ins("ASL Zero Page", 2),
            0x08 => Ins("PHP", 1, Implicit()),

            0x10 => Ins("BPL", 2, Relative()),

            0x20 => Ins("JSR", 3, Absolute()),
            //0x09 => Ins("ORA Immediate", 2),
            //0x0A => Ins("ASL Accumulator", 1),
            //0x0D => Ins("ORA Absolute", 3),
            //0x0E => Ins("ASL Absolute", 3),
            0x78 => Ins("SEI", 1, Implicit()),

            0x8D => Ins("STA", 3, Absolute()),

            0x9A => Ins("TXS", 1, Implicit()),

            0xA0 => Ins("LDY", 2, Immediate()),
            0xA2 => Ins("LDX", 2, Immediate()),
            0xA9 => Ins("LDA", 2, Immediate()),
            0xAD => Ins("LDA", 3, Absolute()),

            0xB0 => Ins("BCS", 2, Relative()),
            0xBD => Ins("LDA", 3, AbsoluteX()),

            0xC9 => Ins("CMP", 2, Immediate()),
            0xCA => Ins("DEX", 1, Implicit()),

            0xD0 => Ins("BNE", 2, Relative()),
            0xD8 => Ins("CLD", 1, Implicit()),
            _ => Ins($"Unknown Opcode: {opcode:X2}", 1)
        };
    }

    public record Instruction(int Address, byte Opcode, string Mnemonic, int Bytes, string? Arguments = null);
}