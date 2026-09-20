using System.Text;

namespace NesAsm.Recompiler;

public static class Output
{
    public static void Rom(string outputPath, IEnumerable<Subroutine> subroutines)
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
        File.WriteAllText(Path.Combine(outputPath, "Rom.txt"), sb.ToString());
    }

    public static void RomMap(string outputPath, IReadOnlyCollection<Subroutine> subroutines, IReadOnlyCollection<Subroutine> potentialSubroutines)
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

        File.WriteAllText(Path.Combine(outputPath, "RomMap.txt"), sb.ToString());
    }

    public static void CandidateSubInstructions(string outputPath, IEnumerable<Subroutine> subroutines)
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
        File.WriteAllText(Path.Combine(outputPath, "CandidateSubIns.txt"), sb.ToString());
    }

    public static void Subroutines(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in subroutines)
        {
            sb.AppendLine($"- {sub.ToString()}");
        }
        File.WriteAllText(Path.Combine(outputPath, "Subroutines.txt"), sb.ToString());
    }

    public static void CandidateSubroutines(string outputPath, IReadOnlyCollection<Subroutine> potentialSubroutines)
    {
        var sb = new StringBuilder();
        foreach (var sub in potentialSubroutines)
        {
            sb.AppendLine($"- {sub.ToString()}");
        }
        File.WriteAllText(Path.Combine(outputPath, "CandiateSubs.txt"), sb.ToString());
    }

    public static void SubroutinesWithUnknown(string outputPath, IEnumerable<Subroutine> subroutines)
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
        File.WriteAllText(Path.Combine(outputPath, "Unknown.txt"), sb.ToString());
    }

    public static void MemoryAccess(string outputPath, IReadOnlyCollection<Subroutine> subroutines)
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
                    .Select(g => $"{(g.Any(r => r.IsRead) ? "R" : " ")}{(g.Any(r => r.IsWrite) ? "W" : " ")}{(g.Any(r => r.IsJump) ? "J" : "")} {Labels.GetLabelAndMemoryAddress(g.Key.Address)}");
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

        File.WriteAllText(Path.Combine(outputPath, "MemoryAccess.txt"), sb.ToString());
    }
}