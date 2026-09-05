using System.Text;

namespace NesAsm.Recompiler;

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

        var nmi = prgRom[^6] + (prgRom[^5] << 8);
        var reset = prgRom[^4] + (prgRom[^3] << 8);
        var irq = prgRom[^2] + (prgRom[^1] << 8);

        Console.WriteLine($"NMI: ${nmi:X4}");
        Console.WriteLine($"Reset: ${reset:X4}");
        Console.WriteLine($"IRQ: ${irq:X4}");

        var functions = Parser.ParsePrgRom(prgRom, reset, nmi);

        OutputFunctions(outputPath, functions);
        OutputInstructions(outputPath, functions);
        OutputFunctionsWithUnknown(outputPath, functions);

        Console.WriteLine($"Total functions: {functions.Count}");
        Console.WriteLine($"Total instructions: {functions.SelectMany(f => f.Instructions).Count()}");
        Console.WriteLine($"Total size: {functions.Sum(f => f.Size)}");
    }

    private static void OutputInstructions(string outputPath, IEnumerable<Function> functions)
    {
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

    private static void OutputFunctions(string outputPath, IEnumerable<Function> functions)
    {
        var sb = new StringBuilder();
        foreach (var func in functions)
        {
            sb.AppendLine($"{func.ToString()} to {func.LastInstructionAddress:X4} (Size: {func.Size})");
            sb.AppendLine($"Nb Instructions: {func.Instructions.Count}");
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(outputPath, "functions.txt"), sb.ToString());
    }

    private static void OutputFunctionsWithUnknown(string outputPath, IEnumerable<Function> functions)
    {
        var sb = new StringBuilder();
        foreach (var func in functions)
        {
            if (func.Instructions.Any(i => i.Mnemonic.Contains("Unknown")))
            {
                sb.AppendLine(func.ToString());
                foreach (var instruction in func.Instructions)
                {
                    sb.AppendLine(instruction.ToString());
                }
                sb.AppendLine();
            }
        }
        File.WriteAllText(Path.Combine(outputPath, "unknown.txt"), sb.ToString());
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
}
