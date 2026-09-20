using System.Text;

namespace NesAsm.Recompiler;

public class MermaidGenerator
{
    public static void GenerateSubRelations(string outputPath, IEnumerable<Subroutine> subroutines)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart LR");
        foreach (var sub in subroutines)
        {
            sb.AppendLine($"    F{sub.Address:X4}[\"${Labels.GetLabelAndMemoryAddress(sub.Address)}\"]");
            foreach (var jump in sub.Jumps)
            {
                sb.AppendLine($"    F{sub.Address:X4} --> F{jump.TargetAddress:X4}");
            }
        }
        sb.AppendLine("```");
        File.WriteAllText(Path.Combine(outputPath, "Subroutines.md"), sb.ToString());
    }

    public static void GenerateRomTreeMap(string outputPath, IEnumerable<Subroutine> subroutines, IEnumerable<Subroutine> potentialSubroutines, int prgSize)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("treemap-beta");

        sb.AppendLine("\"Code\"");
        foreach (var sub in subroutines)
        {
            sb.AppendLine($"    \"{sub.Address:X4}\" : {sub.Size}");
        }

        sb.AppendLine("\"Potential Code\"");
        foreach (var sub in potentialSubroutines)
        {
            sb.AppendLine($"    \"{sub.Address:X4}\" : {sub.Size}");
        }

        sb.AppendLine("\"Data\"");

        sb.AppendLine("\"Unknown\"");
        var codeSize = subroutines.Sum(f => f.Size) + potentialSubroutines.Sum(s => s.Size);
        sb.AppendLine($"    \"Unmapped\" : {prgSize - codeSize}");

        sb.AppendLine("```");
        File.WriteAllText(Path.Combine(outputPath, "RomTreeMap.md"), sb.ToString());
    }
}