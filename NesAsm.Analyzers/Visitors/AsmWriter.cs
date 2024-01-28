using System;
using System.Text;

namespace NesAsm.Analyzers.Visitors;

internal class AsmWriter
{
    private readonly StringBuilder _sb;

    public AsmWriter()
    {
        _sb = new StringBuilder();
        _sb.AppendLine("; Auto generated code using the NesAsm project");
    }

    public void StartHeaderSegment(byte prgRomBanks, byte chrRomBanks, byte mapper, bool verticalMirroring)
    {
        _sb.AppendLine(".segment \"HEADER\"");
        _sb.AppendLine("  .byte $4E, $45, $53, $1A  ; iNES header identifier");
        _sb.AppendLine($"  .byte {prgRomBanks}                   ; {prgRomBanks}x 16KB PRG-ROM Banks");
        _sb.AppendLine($"  .byte {chrRomBanks}                   ; {chrRomBanks}x  8KB CHR-ROM");
        _sb.AppendLine($"  .byte ${mapper:X2}                 ; mapper 0 - $01");
        _sb.AppendLine($"  .byte {(verticalMirroring ? "$00" : "$01")}                 ; $00 - vertical mirroring");
        _sb.AppendLine("");
    }

    public void StartVectorsSegment()
    {
        _sb.AppendLine(".segment \"VECTORS\"");
        _sb.AppendLine("  .addr nmi");
        _sb.AppendLine("  .addr reset");
        _sb.AppendLine("  .addr 0");
        _sb.AppendLine("");
    }

    public void StartStartupSegment()
    {
        _sb.AppendLine(".segment \"STARTUP\"");
        _sb.AppendLine("");
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

    public void IncludeFile(string filepath) => _sb.AppendLine($".include \"{filepath}\"");

    public void StartProc(string procName) => _sb.AppendLine($".proc {Utilities.GetProcName(procName)}");
    
    public void EndProc()
    {
        _sb.AppendLine("");
        _sb.AppendLine("  rts");
        _sb.AppendLine(".endproc");
        _sb.AppendLine("");
    }

    public void StartNmi() => _sb.AppendLine($"nmi:");

    public void EndNmi()
    {
        _sb.AppendLine($"  rti");
        _sb.AppendLine($"");
    }

    public void WriteChars(int[] charBytes)
    {
        int i = 0;
        foreach (var charByte in charBytes)
        {
            _sb.AppendLine($"  .byte {charByte}");

            if (++i % 8 == 0) WriteEmptyLine();
        }

        WriteEmptyLine();
    }

    public void WriteComment(string comment) => _sb.AppendLine($"  ;{comment}");
    
    public void WriteLabel(string label) => _sb.AppendLine($"@{label}:");
    public void WriteVariableLabel(string variableLabel) => _sb.AppendLine($"{variableLabel}:");

    public void WriteOpCode(string opCode) => _sb.AppendLine($"  {opCode}");
    public void WriteOpCode(string opCode, byte address) => _sb.AppendLine($"  {opCode} {address}");
    public void WriteOpCode(string opCode, string address) => _sb.AppendLine($"  {opCode} {address}");
    public void WriteOpCode(string opCode, string baseAddress, string indexorRegister) => _sb.AppendLine($"  {opCode} {baseAddress}, {indexorRegister}");

    public void WriteOpCodeImmediate(string opCode, byte value) => _sb.AppendLine($"  {opCode} #{value}");
    public void WriteOpCodeImmediate(string opCode, string value) => _sb.AppendLine($"  {opCode} #{value}");

    public void WriteJSROpCode(string procName) => _sb.AppendLine($"  jsr {procName}");
    public void WriteJMPOpCode(string procName) => _sb.AppendLine($"  jmp {procName}");
    public void WriteJMPToLabelOpCode(string label) => _sb.AppendLine($"  jmp @{label}");

    public void WriteBranchOpCode(string opCode, string label) => _sb.AppendLine($"  {opCode} @{label}");

    public override string ToString() => _sb.ToString();
}
