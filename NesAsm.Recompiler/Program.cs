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
        LoadSymbols(outputPath);
        var dynamicDispatchAddresses = LoadDynamicDispatchs(outputPath);

        var cart = new Cart(romPath);

        var subroutines = Parser.ParsePrgRom(cart, dynamicDispatchAddresses);
        var potentialSubroutines = Parser.TryParseEmptyPrgRomRange(cart, subroutines);

        OutputSubroutines(outputPath, subroutines);
        OutputSubroutinesWithUnknown(outputPath, subroutines);
        OutputPotentialSubroutines(outputPath, potentialSubroutines);
        OutputRom(outputPath, subroutines);
        OutputRomMap(outputPath, subroutines, potentialSubroutines);
        OutputPotentialSubInstructions(outputPath, potentialSubroutines);
        OutputMemoryAccess(outputPath, subroutines);

        Console.WriteLine($"Total subroutines: {subroutines.Count}");
        Console.WriteLine($"Total instructions: {subroutines.SelectMany(f => f.Instructions).Count()}");
        Console.WriteLine($"Total size: {subroutines.Sum(f => f.Size)}");

        MermaidGenerator.GenerateSubRelations(outputPath, subroutines);
        MermaidGenerator.GenerateRomTreeMap(outputPath, subroutines, potentialSubroutines, cart.PrgSize);

        new Runner(cart).Run(subroutines);
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

    private static IEnumerable<ushort> LoadDynamicDispatchs(string outputPath)
    {
        var dispatchs = new List<ushort>();

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
                    dispatchs.Add((ushort)addr);
                }
            }
        }
        else
        {
            File.Create(labelsFilePath);
        }

        return dispatchs;
    }

    private static void LoadSymbols(string outputPath)
    {
        var labelsFilePath = Path.Combine(outputPath, "symbols.sym");
        if (File.Exists(labelsFilePath))
        {
            var lines = File.ReadAllLines(labelsFilePath);
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith('#') || string.IsNullOrWhiteSpace(line))
                {
                    // Skip
                }
                else
                {
                    var parts = line.Split(' ');
                    var address = parts[0]!;
                    var label = parts[1];
                    var type = parts[2];

                    if (type == "func" || type == "ram")
                        if (int.TryParse(address, System.Globalization.NumberStyles.HexNumber, null, out var addr))
                            if (string.IsNullOrWhiteSpace(Labels.GetLabel(addr)))
                                Labels.AddMemoryLabel(addr, label);
                }
            }
        }
    }

    private static void OutputRom(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in subroutines)
        {
            sb.AppendLine(sub.ToString());
            foreach (var instruction in sub.Instructions)
            {
                var branch = sub.Branches.FirstOrDefault(b => b.TargetAddress == instruction.Address);
                var label = branch != null ? $"{Labels.GetLabelOrMemoryAddress(branch.TargetAddress)}:" : string.Empty;

                var memoryAccess = instruction.GetMemoryAccess(sub);
                sb.AppendLine($"{label,-15}{instruction.ToString(),-40}{memoryAccess}");
            }
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(outputPath, "rom.txt"), sb.ToString());
    }

    private static void OutputRomMap(string outputPath, IReadOnlyCollection<Subroutine> subroutines, IReadOnlyCollection<Subroutine> potentialSubroutines)
    {
        var sb = new StringBuilder();

        var memAccessAddress = subroutines.SelectMany(s => s.GetMemoryAccess())
            .Where(m => m.IsRead || m.IsWrite).Select(m => m.TargetAddress)
            .Distinct().Where(a => a >= 0x8000).ToList();

        var potentialMemAccessAddress = potentialSubroutines.SelectMany(s => s.GetMemoryAccess())
            .Where(m => m.IsRead || m.IsWrite).Select(m => m.TargetAddress)
            .Distinct().Where(a => a >= 0x8000).Except(memAccessAddress);

        var regions = subroutines.Select(s => new { Type = "sub", IsOfficial = " ", s.Address, s.Size, s.Label })
            .Concat(potentialSubroutines.Select(s => new { Type = "sub", IsOfficial = "?", s.Address, s.Size, s.Label }))
            .Concat(memAccessAddress.Select(a => new { Type = "dat", IsOfficial = " ", Address = a, Size = 1, Label = Labels.GetLabel(a) }))
            .Concat(potentialMemAccessAddress.Select(a => new { Type = "dat", IsOfficial = "?", Address = a, Size = 1, Label = Labels.GetLabel(a) }))
            .OrderBy(x => x.Address);

        var overlapping = new List<(ushort Address1, int LastAddress1, ushort Address2, int LastAddress2)>();
        (ushort Address, int LastAddress) lastRegion = (0, 0);
        foreach (var r in regions)
        {
            sb.AppendLine($"${r.Address:X4}-${r.Address + r.Size - 1:X4} l:{r.Size,4} {r.IsOfficial} {r.Type} {r.Label}");

            var region = (r.Address, r.Address + r.Size - 1);
            if (region.Address <= lastRegion.LastAddress)
            {
                overlapping.Add((lastRegion.Address, lastRegion.LastAddress, region.Address, region.Item2));
            }

            lastRegion = region;
        }

        sb.AppendLine();
        sb.AppendLine("----------------");
        sb.AppendLine();
        sb.AppendLine("Overlapping memory regions:");
        sb.AppendLine();

        foreach (var o in overlapping)
        {
            sb.AppendLine($"${o.Address1:X4}-${o.LastAddress1:X4} and ${o.Address2:X4}-${o.LastAddress2:X4}");
        }

        File.WriteAllText(Path.Combine(outputPath, "rommap.txt"), sb.ToString());
    }

    private static void OutputPotentialSubInstructions(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in subroutines)
        {
            sb.AppendLine(sub.ToString());
            foreach (var instruction in sub.Instructions)
            {
                var branch = sub.Branches.FirstOrDefault(b => b.TargetAddress == instruction.Address);
                var label = branch != null ? $"{Labels.GetLabelOrMemoryAddress(branch.TargetAddress)}:" : string.Empty;

                var memoryAccess = instruction.GetMemoryAccess(sub);
                sb.AppendLine($"{label,-15}{instruction.ToString(),-40}{memoryAccess}");
            }
            sb.AppendLine();
        }
        File.WriteAllText(Path.Combine(outputPath, "potentialsubins.txt"), sb.ToString());
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

    private static void OutputPotentialSubroutines(string outputPath, IReadOnlyCollection<Subroutine> potentialSubroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in potentialSubroutines)
        {
            sb.AppendLine($"- {sub.ToString()}");
        }
        File.WriteAllText(Path.Combine(outputPath, "potentialsubs.txt"), sb.ToString());
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

    private static void OutputMemoryAccess(string outputPath, IReadOnlyCollection<Subroutine> subroutines)
    {
        var sb = new StringBuilder();

        void PrintMemoryAccess(IEnumerable<MemoryAccessRecord> memoryAccesRecords)
        {
            MemoryRegion? currentRegion = null;
            int index = 0;
            foreach (var item in memoryAccesRecords.GroupBy(m => m.TargetAddress).OrderBy(a => a.Key))
            {
                var region = item.First().MemoryRegion;
                if (item.First().MemoryRegion != currentRegion)
                {
                    currentRegion = region;
                    sb.AppendLine();
                    sb.AppendLine($"-- {region} --");
                }
                var target = Labels.GetLabelAndMemoryAddress(item.Key);
                if (++index == 4)
                {
                    index = 0;
                    target = $"{target} ".PadRight(25, '.')[..25];
                }
                var sourceSub = item
                    .GroupBy(m => m.Subroutine)
                    .OrderBy(g => g.Key.Address)
                    .Select(g => $"{(g.Any(r => r.IsRead) ? "R": " ")}{(g.Any(r => r.IsWrite) ? "W" : " ")}{(g.Any(r => r.IsJump) ? "J" : "")} {Labels.GetLabelAndMemoryAddress(g.Key.Address)}");
                sb.AppendLine($"{target,-25} : {string.Join("  ", sourceSub)}");
            }
        }

        void PrintSeparator()
        {
            sb.AppendLine();
            sb.AppendLine("------------------------------------------------");
            sb.AppendLine();
        }

        sb.AppendLine("===== Direct Access =====");
        PrintMemoryAccess(subroutines.SelectMany(s => s.GetDirectAccess()));

        PrintSeparator();

        sb.AppendLine("===== Indirect Access =====");
        PrintMemoryAccess(subroutines.SelectMany(s => s.GetIndirectAccess()));

        PrintSeparator();

        sb.AppendLine("===== Dynamic Dispatch =====");
        PrintMemoryAccess(subroutines.SelectMany(s => s.GetDynamicDispatch()));

        File.WriteAllText(Path.Combine(outputPath, "memoryaccess.txt"), sb.ToString());
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
