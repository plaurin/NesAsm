using System.Text;

namespace NesAsm.Analyzers.Visitors;

internal class AsmWriter
{
    private readonly StringBuilder _sb;

    public AsmWriter()
    {
        _sb = new StringBuilder();
    }

    public void StartCodeSegment()
    {
        _sb.AppendLine(".segment \"CODE\"");
        _sb.AppendLine("");
    }

    public void StartCharsSegment()
    {
        _sb.AppendLine(".segment \"CHARS\"");
        _sb.AppendLine("");
    }

    public void Write(string value) => _sb.Append(value);

    public void WriteEmptyLine() => _sb.AppendLine("");

    public void IncludeFile(string filepath) => _sb.AppendLine($".include {filepath}");

    public void StartProc(string procName) => _sb.AppendLine($".proc {MethodVisitor.GetProcName(procName)}");
    
    public void EndProc()
    {
        _sb.AppendLine("");
        _sb.AppendLine("  rts");
        _sb.AppendLine(".endproc");
        _sb.AppendLine("");
    }

    public void WriteChars(int[] charBytes)
    {
        int i = 0;
        foreach (var charByte in charBytes)
        {
            _sb.AppendLine($"  .byte %{charByte:B8}");

            if (++i % 8 == 0) _sb.AppendLine("");
        }
    }

    public void WriteComment(string comment) => _sb.AppendLine($"  ;{comment}");
    
    public void WriteLabel(string label) => _sb.AppendLine($"@{label}:");

    public void WriteOpCode(string opCode) => _sb.AppendLine($"  {opCode}");
    public void WriteOpCode(string opCode, byte address) => _sb.AppendLine($"  {opCode} {address}");
    public void WriteOpCode(string opCode, string address) => _sb.AppendLine($"  {opCode} {address}");

    public void WriteOpCodeImmediate(string opCode, byte value) => _sb.AppendLine($"  {opCode} #{value}");
    public void WriteOpCodeImmediate(string opCode, string value) => _sb.AppendLine($"  {opCode} #{value}");

    public void WriteJSROpCode(string procName) => _sb.AppendLine($"  jsr {procName}");
    
    public void WriteBranchOpCode(string opCode, string label) => _sb.AppendLine($"  {opCode} @{label}");

    public override string ToString() => _sb.ToString();

}
