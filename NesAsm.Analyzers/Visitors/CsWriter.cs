using System;
using System.Linq;
using System.Text;

namespace NesAsm.Analyzers.Visitors;

internal class CsWriter
{
    private readonly StringBuilder _sb;
    private int _indentation = 0;

    public CsWriter()
    {
        _sb = new StringBuilder();
        _sb.AppendLine("// Auto generated code using the NesAsm project");
    }

    private string CurrentIndentation => "".PadRight(_indentation);

    public void WriteWithIndentation(string value) => _sb.Append($"{CurrentIndentation}{value}");

    public void WriteLineWithIndentation(string value) => _sb.AppendLine($"{CurrentIndentation}{value}");

    public void WriteEmptyLine() => _sb.AppendLine();

    public void WriteComment(string comment) => _sb.AppendLine($"{CurrentIndentation}// {comment}");

    internal void WriteStartBlock()
    {
        WriteLineWithIndentation("{");
        IncreaseIndentation();
    }

    internal void WriteEndBlock()
    {
        DecreaseIndentation();
        WriteLineWithIndentation("}");
    }

    internal void IncreaseIndentation() => _indentation += 4;

    internal void DecreaseIndentation() => _indentation = Math.Max(0, _indentation - 4);

    internal void WritePaletteColorsData(byte[] nesColors)
    {
        WriteWithIndentation($"{string.Join(", ", nesColors.Select(nc => $"0x{nc:X2}"))},");
    }

    internal void WriteEndOfLineComment(string comment) => _sb.AppendLine($" // {comment}");

    public override string ToString() => _sb.ToString();
}
