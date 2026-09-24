using NesAsm.Emulator;

namespace NesAsm.Recompiler;

internal class Program
{
    private static void Main(string[] args)
    {
        var (gameName, romPath, outputPath) = ParseArguments(args);

        Console.WriteLine($"Parsing game {gameName} at {Path.GetFileNameWithoutExtension(romPath)}");
        Console.WriteLine($"Output path: {outputPath}");

        Directory.CreateDirectory(outputPath);

        Input.LoadCustomLabels(outputPath);
        Input.LoadSymbols(outputPath);
        var dynamicDispatchAddresses = Input.LoadDynamicDispatchs(outputPath);

        var cart = new Cart(romPath);

        IReadOnlyCollection<Subroutine>? subroutines = null;

        for (int iteration = 1; iteration <= 1; iteration++)
        {
            var iterationPath = Path.Combine(outputPath, $"Iteration{iteration}");
            Directory.CreateDirectory(iterationPath);
            subroutines = ProcessIteration(cart, dynamicDispatchAddresses, iteration, iterationPath);
        }

        var runner = new Runner(cart);
        try
        {
            runner.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        var runPath = Path.Combine(outputPath, $"Run1");
        Directory.CreateDirectory(runPath);
        Output.Run(runPath, runner);
    }

    private static IReadOnlyCollection<Subroutine> ProcessIteration(Cart cart, IEnumerable<ushort> extraAddressToParse, int iteration, string outputPath)
    {
        Console.WriteLine();
        Console.WriteLine($"Processing iteration: {iteration}");
        Console.WriteLine();

        // Read input for previous iterations
        // TODO

        // Parse
        var subroutines = Parser.ParsePrgRom(cart, extraAddressToParse);

        Output.Subroutines(outputPath, subroutines);
        Output.SubroutinesWithUnknown(outputPath, subroutines);
        Output.Rom(outputPath, subroutines);
        Output.MemoryAccess(outputPath, subroutines);

        MermaidGenerator.GenerateSubRelations(outputPath, subroutines);

        Console.WriteLine($"Total subroutines: {subroutines.Count}");
        Console.WriteLine($"Total instructions: {subroutines.SelectMany(f => f.Instructions).Count()}");
        Console.WriteLine($"Total size: {subroutines.Sum(f => f.Size)}");

        // Find candidate subs
        Console.WriteLine();
        Console.WriteLine("Trying to parse empty space");
        Console.WriteLine();

        var candidateSubroutines = Parser.TryParseEmptyPrgRomRange(cart, subroutines);

        var promotedSubs = Output.CandidateSubroutines(outputPath, subroutines, candidateSubroutines);
        Output.CandidateSubInstructions(outputPath, candidateSubroutines, promotedSubs);
        Output.RomMap(outputPath, subroutines, candidateSubroutines);

        MermaidGenerator.GenerateRomTreeMap(outputPath, subroutines, candidateSubroutines, cart.PrgSize);

        return subroutines;
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