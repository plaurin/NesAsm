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

        //IReadOnlyCollection<Subroutine>? subroutines = null;

        //for (int iteration = 1; iteration <= 1; iteration++)
        //{
        //    var iterationPath = Path.Combine(outputPath, $"Iteration{iteration}");
        //    Directory.CreateDirectory(iterationPath);
        //    subroutines = ProcessIteration(cart, dynamicDispatchAddresses, iteration, iterationPath);
        //}

        var runner = new Runner(cart);
        try
        {
            runner.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            var runPath = Path.Combine(outputPath, $"Run1");
            Directory.CreateDirectory(runPath);
            Output.Run(runPath, runner);
        }

        var addresses = FindIndirectJumpAddresses(runner);
        foreach (var item in addresses)
        {
            Console.WriteLine(Labels.GetLabelAndMemoryAddress(item));
        }
    }

    private static IEnumerable<ushort> FindIndirectJumpAddresses(Runner runner)
    {
        var indirectJumpSubs = runner.Subroutines.Where(s => s.Instructions.Any(i => Instruction.IsDynamicDispatch(i.Opcode))).ToList();

        var useIndirectJumpsSub = runner.DirectJumpTableMemoryAccess().Where(m => indirectJumpSubs.Any(s => s.Address == m.TargetAddress)).ToList();

        foreach (var sub in useIndirectJumpsSub)
        {
            var nextAddress = sub.Instruction.Address + sub.Instruction.Bytes;
            var nextSub = runner.Subroutines.FirstOrDefault(s => s.Address > nextAddress);

            if (nextSub != null)
            {
                while(nextAddress < nextSub.Address)
                {
                    var potentialAddress = runner.Cpu.Memory.PeekWordArgument((ushort)(nextAddress - 1));
                    if (potentialAddress < 0x8000 || potentialAddress > 0xFFF9) break;
                    yield return potentialAddress;
                    nextAddress += 2;
                }
            }
            else
            {
                Console.WriteLine($"Sub {sub.Subroutine.LabelOrAddress} at ${sub.Subroutine.Address:X4} uses indirect jump to ${sub.TargetAddress:X4}, no next sub found");
            }
        }
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