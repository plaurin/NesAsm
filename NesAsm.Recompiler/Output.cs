using System.Text;

namespace NesAsm.Recompiler;

public static class Output
{
    public static void Rom(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        var addr = 0x8000;

        foreach (var sub in subroutines)
        {
            if (addr < sub.Address)
            {
                sb.AppendLine($"Unknown at ${addr:X4} to ${sub.Address - 1:X4} (Size: {sub.Address - addr + 1})");
                sb.AppendLine();
            }

            sb.AppendLine(sub.ToString());
            int nextInstruction = sub.Address;
            foreach (var instruction in sub.Instructions)
            {
                if (instruction.Address != nextInstruction)
                    sb.AppendLine($"                 [${nextInstruction:X4}..${instruction.Address - 1:X4}] ?");

                var branch = sub.Branches.FirstOrDefault(b => b.TargetAddress == instruction.Address);
                var label = branch != null ? $"{Labels.GetLabelOrMemoryAddress(branch.TargetAddress)}:" : string.Empty;

                var memoryAccess = instruction.GetMemoryAccess(sub);
                sb.AppendLine($"{label,-15}{instruction.ToString(),-40}{memoryAccess}");
                nextInstruction = instruction.Address + instruction.Bytes;
            }
            sb.AppendLine();

            addr = sub.Address + sub.Size;
        }

        if (addr < 0xFFFA)
        {
            sb.AppendLine($"Unknown at ${addr:X4} to $FFF9 (Size: {0xFFFA - addr + 1})");
        }

        File.WriteAllText(Path.Combine(outputPath, "Rom.txt"), sb.ToString());
    }

    public static void Rom2(string outputPath, IEnumerable<MemoryRegion> memoryRegions)
    {
        var sb = new StringBuilder();

        foreach (var region in memoryRegions)
        {
            if (region is Subroutine sub)
            {
                sb.AppendLine(sub.ToString());
                foreach (var instruction in sub.Instructions)
                {
                    var branch = sub.Branches.FirstOrDefault(b => b.TargetAddress == instruction.Address);
                    var label = branch != null ? $"{Labels.GetLabelOrMemoryAddress(instruction.Address)}:" : string.Empty;

                    var memoryAccess = instruction.GetMemoryAccess(sub);
                    sb.AppendLine($"{label,-15}{instruction.ToString(),-40}{memoryAccess}");
                }
                sb.AppendLine();
            }
            else
            {
                throw new NotSupportedException($"MemoryRegion of type {region.GetType().Name} is not supported in Rom2 output.");
            }
        }

        File.WriteAllText(Path.Combine(outputPath, "Rom2.txt"), sb.ToString());
    }

    public static void RomMap(string outputPath, IReadOnlyCollection<Subroutine> subroutines, IEnumerable<Subroutine> potentialSubroutines)
    {
        var sb = new StringBuilder();

        var memAccessAddress = subroutines.SelectMany(s => s.GetMemoryAccess())
            .Where(m => m.IsRead || m.IsWrite).Select(m => m.TargetAddress)
            .Distinct().Where(a => a >= 0x8000).ToList();

        potentialSubroutines = potentialSubroutines.Where(s => !s.InvalidInstructions.Any());

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

    public static void CandidateSubInstructions(string outputPath, IEnumerable<Subroutine> candidateSubroutines, IReadOnlyCollection<Subroutine> promotedSubs)
    {
        var sb = new StringBuilder();
        foreach (var sub in candidateSubroutines)
        {
            var isPromoted = promotedSubs.Contains(sub);
            sb.AppendLine($"{sub.ToString()} {(isPromoted ? "** Promoted **" : "")}");
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

    public static IReadOnlyCollection<Subroutine> CandidateSubroutines(string outputPath, IReadOnlyCollection<Subroutine> existingSubroutines, IReadOnlyCollection<Subroutine> candidateSubroutines)
    {
        static string ListSub(IEnumerable<MemoryAccessRecord> memoryAccess, IReadOnlyCollection<Subroutine> subs)
        {
            return string.Join(" ", memoryAccess.Select(m => subs.FirstOrDefault(s => s.IsInSub(m.TargetAddress))?.LabelOrAddress ?? $"${m.LabelAndAddress}"));
        }

        static string ListExistingAndCandidate(IEnumerable<MemoryAccessRecord> existingMemoryAcces, IReadOnlyCollection<Subroutine> existingSubs,
            IEnumerable<MemoryAccessRecord> candidateMemoryAcces, IReadOnlyCollection<Subroutine> candidateSubs)
        {
            if (!(existingMemoryAcces.Any() || candidateMemoryAcces.Any())) return "0";

            return (existingMemoryAcces.Any() ? ListSub(existingMemoryAcces, existingSubs) : "") +
                (candidateMemoryAcces.Any() ? $" - Candidate {ListSub(candidateMemoryAcces, candidateSubs)}" : "");
        }

        static string ListOrZero(IEnumerable<string> items)
        {
            if (items.Any())
                return string.Join(" ", items);

            return "0";
        }

        var existingJumps = existingSubroutines.SelectMany(s => s.GetMemoryAccess()).Where(m => m.IsJump).ToList();
        var candidateJumps = candidateSubroutines.SelectMany(s => s.GetMemoryAccess()).Where(m => m.IsJump).ToList();

        var result = new List<Subroutine>();
        var sb = new StringBuilder();
        foreach (var sub in candidateSubroutines)
        {
            sb.AppendLine($"- {sub.ToString()}");

            var existingJumpsToThisSub = existingJumps.Where(m => sub.IsInSub(m.TargetAddress));
            var candidateJumpsToThisSub = candidateJumps.Where(m => sub.IsInSub(m.TargetAddress));

            sb.AppendLine($"   Jump to this sub:    {ListExistingAndCandidate(existingJumpsToThisSub, existingSubroutines, candidateJumpsToThisSub, candidateSubroutines)}");

            var memoryAccess = sub.GetMemoryAccess().ToList();

            var jumps = memoryAccess.Where(m => m.IsJump).ToList();
            var jumpsInExistingSub = jumps.Where(j => existingSubroutines.Any(s => s.IsInSub(j.TargetAddress))).ToList();
            var jumpsInCandidateSub = jumps.Except(jumpsInExistingSub).Where(j => candidateSubroutines.Any(s => s.IsInSub(j.TargetAddress))).ToList();
            var orphanJumps = jumps.Except(jumpsInExistingSub).Except(jumpsInCandidateSub).Select(m => m.LabelAndAddress);

            sb.AppendLine($"   Jumps to other sub:  {ListExistingAndCandidate(jumpsInExistingSub, existingSubroutines, jumpsInCandidateSub, candidateSubroutines)}");
            sb.AppendLine($"   ! Orphan Jumps:      {ListOrZero(orphanJumps)}");

            var ramAccess = memoryAccess.Where(m => m.TargetAddress <= 0x7FF).Select(m => m.LabelAndAddress);
            var romAccess = memoryAccess.Where(m => m.TargetAddress >= 0x6000).Select(m => m.LabelAndAddress);
            var unknownAccess = memoryAccess.Where(m => m.MemoryRegion == MemoryRegionKind.Unknown).Select(m => m.LabelAndAddress);

            sb.AppendLine($"   Memory Access (RAM): {ListOrZero(ramAccess)}");
            sb.AppendLine($"   Memory Access (ROM): {ListOrZero(romAccess)}");
            sb.AppendLine($"   ! Memory Access:     {ListOrZero(unknownAccess)}");

            var facts = new List<(string Reason, int Score)>();
            if (sub.InvalidInstructions.Any()) facts.Add(("Invalid instructions!", -100));
            if (existingJumpsToThisSub.Any()) facts.Add(("Existing jumps to this sub", 40));
            if (candidateJumpsToThisSub.Any()) facts.Add(("Existing jumps to this sub", 5));
            if (jumpsInExistingSub.Any()) facts.Add(("Existing jumps to this sub", 15));
            if (jumpsInCandidateSub.Any()) facts.Add(("Existing jumps to this sub", 5));
            if (orphanJumps.Any()) facts.Add(("Orphan jumps", -10));
            if (ramAccess.Any()) facts.Add(("RAM Access", 10));
            if (romAccess.Any()) facts.Add(("ROM Access", 10));

            var total = facts.Sum(f => f.Score);
            var pass = total > 30;
            if (pass) result.Add(sub);

            sb.AppendLine($"   => Final decision: {(pass ? "Pass!" : "Fail")} [{total}] {string.Join("; ", facts.Select(f => f.Reason))}");
            sb.AppendLine();
        }

        File.WriteAllText(Path.Combine(outputPath, "CandiateSubs.txt"), sb.ToString());

        var promotedSubs = result
            .SelectMany(r => r.Jumps.Select(j => j.TargetAddress))
            .Distinct()
            .Where(a => !existingSubroutines.Any(s => s.IsInSub(a)))
            .OrderBy(a => a)
            .Select(a => a.ToString("X4"));

        File.WriteAllLines(Path.Combine(outputPath, "PromotedSubs.txt"), promotedSubs);

        return result;
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

    public static void MemoryAccess(string outputPath, IEnumerable<Subroutine> subroutines, IEnumerable<MemoryAccessRecord>? dynamicDispatchOverride = null)
    {
        var sb = new StringBuilder();

        void PrintMemoryAccess(IEnumerable<MemoryAccessRecord> memoryAccesRecords)
        {
            MemoryRegionKind? currentRegion = null;
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
        if (dynamicDispatchOverride != null)
            PrintMemoryAccess(dynamicDispatchOverride);
        else
            PrintMemoryAccess(subroutines.SelectMany(s => s.GetDynamicDispatch()));

        File.WriteAllText(Path.Combine(outputPath, "MemoryAccess.txt"), sb.ToString());
    }

    public static void MemoryAccess2(string outputPath, IEnumerable<MemoryAccessRecord> reads, IEnumerable<MemoryAccessRecord> writes, IEnumerable<MemoryAccessRecord> directJumps, IEnumerable<MemoryAccessRecord> indirectJumps)
    {
        var sb = new StringBuilder();

        var allAccess = reads.Concat(writes).Concat(directJumps).Concat(indirectJumps).ToList();

        void PrintMemoryAccess(IEnumerable<MemoryAccessRecord> memoryAccesRecords)
        {
            MemoryRegionKind? currentRegion = null;
            int index = 0;
            foreach (var memoryAccess in memoryAccesRecords.OrderBy(a => a.TargetAddress).ThenBy(a => a.RomAddress))
            {
                var region = memoryAccess.MemoryRegion;
                if (memoryAccess.MemoryRegion != currentRegion)
                {
                    currentRegion = region;
                    sb.AppendLine();
                    sb.AppendLine($"-- {region} --");
                }

                var target = Labels.GetLabelAndMemoryAddress(memoryAccess.TargetAddress);

                if (memoryAccess.Size > 1)
                    target = $"{target}-{memoryAccess.TargetAddress + memoryAccess.Size - 1:X4} (s: {memoryAccess.Size})";

                if (++index == 4)
                {
                    index = 0;
                    target = $"{target} ".PadRight(35, '.')[..35];
                }

                var sourceSub = $"{(memoryAccess.IsRead ? "R" : " ")}{(memoryAccess.IsWrite ? "W" : " ")}{(memoryAccess.IsJump ? "J" : "")} ${memoryAccess.Instruction.Address:X4}-{Labels.GetLabelAndMemoryAddress(memoryAccess.Subroutine.Address)}";
                sb.AppendLine($"{target,-35} : {string.Join("  ", sourceSub)}");
            }
        }

        void PrintSeparator()
        {
            sb.AppendLine();
            sb.AppendLine("------------------------------------------------");
            sb.AppendLine();
        }

        sb.AppendLine("===== Direct Access =====");
        PrintMemoryAccess(allAccess.Where(a => a.IsDirectAccess && !a.IsJump));

        PrintSeparator();

        sb.AppendLine("===== Indirect Access =====");
        PrintMemoryAccess(allAccess.Where(a => !a.IsDirectAccess && !a.IsJump));

        PrintSeparator();

        sb.AppendLine("===== Direct Jumps =====");
        PrintMemoryAccess(allAccess.Where(a => a.IsDirectAccess && a.IsJump));

        PrintSeparator();

        sb.AppendLine("===== Indirect Jumps =====");
        PrintMemoryAccess(allAccess.Where(a => !a.IsDirectAccess && a.IsJump));

        File.WriteAllText(Path.Combine(outputPath, "MemoryAccess2.txt"), sb.ToString());
    }

    public static void Run(string outputPath, Runner runner)
    {
        var subroutines = runner.Subroutines;

        Subroutines(outputPath, subroutines);
        Rom(outputPath, subroutines);
        Rom2(outputPath, runner.MemoryRegions);
        MemoryAccess(outputPath, subroutines, runner.IndirectJumpTableMemoryAccess());
        MemoryAccess2(outputPath, runner.Reads(), runner.Writes(), runner.DirectJumpTableMemoryAccess(), runner.IndirectJumpTableMemoryAccess());

        MermaidGenerator.GenerateSubRelations(outputPath, subroutines);

        Callstacks(outputPath, runner.Callstacks);

        BranchesPathNotTaken(outputPath, subroutines);
    }

    public static void Callstacks(string outputPath, IEnumerable<string> callstacks)
    {
        var sb = new StringBuilder();
        foreach (var line in callstacks)
        {
            sb.AppendLine(line);
        }

        sb.AppendLine();
        sb.AppendLine("--------------------------------");
        sb.AppendLine();
        sb.AppendLine("=== Per Frames ===");
        sb.AppendLine();

        foreach (var line in callstacks.Where(l => l.StartsWith("  * Total")))
        {
            sb.AppendLine(line);
        }

        File.WriteAllText(Path.Combine(outputPath, "Callstacks.txt"), sb.ToString());
    }

    private static void BranchesPathNotTaken(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();

        foreach (var subroutine in subroutines)
        {
            var hasPrintSub = false;
            foreach (var branch in subroutine.Branches)
            {
                var hasTakenBranch = subroutine.Instructions.Any(i => i.Address == branch.TargetAddress);
                var hasBranchSkiped = subroutine.Instructions.Any(i => i.Address == branch.Address + 2);
                
                if (!hasTakenBranch || !hasBranchSkiped)
                {
                    if (!hasPrintSub)
                    {
                        sb.AppendLine(subroutine.ToString());
                        hasPrintSub = true;
                    }
                    sb.AppendLine($"  ${branch.Address:X4} {(!hasBranchSkiped ? "not skipped branch ": "")}{(!hasTakenBranch ? $"not taken branch {Labels.GetLabelAndMemoryAddress(branch.TargetAddress)}" : "")}");
                }
            }
            if (hasPrintSub) sb.AppendLine();
        }

        File.WriteAllText(Path.Combine(outputPath, "Branches.txt"), sb.ToString());
    }
}
