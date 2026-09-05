using System.Text;

namespace NesAsm.Recompiler;

public class MermaidGenerator
{
    public static void Generate(string outputPath, IEnumerable<Function> functions)
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
        File.WriteAllText(Path.Combine(outputPath, "functions.md"), sb.ToString());
    }
}