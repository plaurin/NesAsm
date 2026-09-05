using System.Text;

namespace NesAsm.Recompiler;

public class MermaidGenerator
{
    public static void GenerateSubRelations(string outputPath, IEnumerable<Subroutine> functions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("flowchart TD");
        foreach (var func in functions)
        {
            sb.AppendLine($"    F{func.Address:X4}[\"${func.Address:X4}\"]");
            foreach (var jump in func.Jumps)
            {
                sb.AppendLine($"    F{func.Address:X4} --> F{jump.TargetAddress:X4}");
            }
        }
        sb.AppendLine("```");
        File.WriteAllText(Path.Combine(outputPath, "Functions.md"), sb.ToString());
    }

    public static void GenerateRomTreeMap(string outputPath, IEnumerable<Subroutine> functions, int prgSize)
    {
        var sb = new StringBuilder();
        sb.AppendLine("```mermaid");
        sb.AppendLine("treemap-beta");
        sb.AppendLine("\"Code\"");

        foreach (var func in functions)
        {
            sb.AppendLine($"    \"{func.Address:X4}\" : {func.Size}");
        }

        sb.AppendLine("\"Data\"");

        sb.AppendLine("\"Unknown\"");
        var codeSize = functions.Sum(f => f.Size);
        sb.AppendLine($"    \"Unmapped\" : {prgSize - codeSize}");

        sb.AppendLine("```");
        File.WriteAllText(Path.Combine(outputPath, "RomTreeMap.md"), sb.ToString());
    }
}