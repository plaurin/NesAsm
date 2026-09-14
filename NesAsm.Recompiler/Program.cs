using NesAsm.Emulator;
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

        LoadCustomLabels(outputPath);
        var dynamicDispatchAddresses = LoadDynamicDispatchs(outputPath);

        var cart = new Cart(romPath);

        var subroutines = Parser.ParsePrgRom(cart, dynamicDispatchAddresses);

        OutputSubroutines(outputPath, subroutines);
        OutputInstructions(outputPath, subroutines);
        OutputSubroutinesWithUnknown(outputPath, subroutines);

        Console.WriteLine($"Total subroutines: {subroutines.Count}");
        Console.WriteLine($"Total instructions: {subroutines.SelectMany(f => f.Instructions).Count()}");
        Console.WriteLine($"Total size: {subroutines.Sum(f => f.Size)}");

        MermaidGenerator.GenerateSubRelations(outputPath, subroutines);
        MermaidGenerator.GenerateRomTreeMap(outputPath, subroutines, cart.PrgSize);

        new Runner().Run(subroutines, cart);
    }

    private static void LoadCustomLabels(string outputPath)
    {
        var labelsFilePath = Path.Combine(outputPath, "labels.txt");
        if (File.Exists(labelsFilePath))
        { 
            var lines = File.ReadAllLines(labelsFilePath);
            foreach (var line in lines)
            {
                var address = line[0..4];
                var label = line[5..].Trim();
                if (int.TryParse(address, System.Globalization.NumberStyles.HexNumber, null, out var addr))
                {
                    Labels.AddMemoryLabel(addr, label);
                }
            }
        }
        else
        {
            File.Create(labelsFilePath);
        }
    }

    private static IEnumerable<int> LoadDynamicDispatchs(string outputPath)
    {
        var dispatchs = new List<int>();

        var labelsFilePath = Path.Combine(outputPath, "dispatchs.txt");
        if (File.Exists(labelsFilePath))
        {
            var lines = File.ReadAllLines(labelsFilePath);
            foreach (var line in lines)
            {
                var sourceAddress = line[0..4];
                var targetAddress = line[5..].Trim();
                if (int.TryParse(targetAddress, System.Globalization.NumberStyles.HexNumber, null, out var addr))
                {
                    dispatchs.Add(addr);
                }
            }
        }
        else
        {
            File.Create(labelsFilePath);
        }

        return dispatchs;
    }

    private static void OutputInstructions(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in subroutines)
        {
            sb.AppendLine(sub.ToString());
            foreach (var instruction in sub.Instructions)
            {
                sb.AppendLine(instruction.ToString());
            }
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(outputPath, "instructions.txt"), sb.ToString());
    }

    private static void OutputSubroutines(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in subroutines)
        {
            sb.AppendLine($"- {sub.ToString()}");
        }
        File.WriteAllText(Path.Combine(outputPath, "subroutines.txt"), sb.ToString());
    }

    private static void OutputSubroutinesWithUnknown(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in subroutines)
        {
            if (sub.Instructions.Any(i => i.Mnemonic.Contains("Unknown")))
            {
                sb.AppendLine(sub.ToString());
                foreach (var instruction in sub.Instructions)
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
