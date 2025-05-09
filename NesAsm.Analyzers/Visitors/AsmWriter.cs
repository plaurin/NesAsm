﻿using System;
using System.Linq;
using System.Text;

namespace NesAsm.Analyzers.Visitors;

internal class AsmWriter
{
    private readonly StringBuilder _sb;
    private int _indentation = 0;

    public AsmWriter()
    {
        _sb = new StringBuilder();
        _sb.AppendLine("; Auto generated code using the NesAsm project");
    }

    private string CurrentIndentation => "".PadRight(_indentation);
    private string CurrentLabelIndentation => "".PadRight(Math.Max(0, _indentation - 2));

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

    public void StartClassScope(string className)
    {
        _sb.AppendLine($".scope {className}");
        _sb.AppendLine("");
        _indentation += 2;
    }

    public void EndClassScope()
    {
        _sb.AppendLine($".endscope");
        _sb.AppendLine("");
        _indentation = Math.Max(0, _indentation - 2);
    }

    public void Write(string value) => _sb.Append(value);

    public void WriteLineWithIndentation(string value) => _sb.AppendLine($"{CurrentIndentation}{value}");

    public void WriteEmptyLine() => _sb.AppendLine();

    public void IncludeFile(string filepath) => _sb.AppendLine($".include \"{filepath}\"");

    public void StartProc(string procName)
    {
        _sb.AppendLine($"{CurrentIndentation}.proc {Utilities.GetProcName(procName)}");
        _indentation += 2;
    }

    public void EndProc()
    {
        WriteEmptyLine();
        _sb.AppendLine($"{CurrentIndentation}rts");

        _indentation -= 2;
        _sb.AppendLine($"{CurrentIndentation}.endproc");
        WriteEmptyLine();
    }

    public void StartNmi() => _sb.AppendLine($"nmi:");

    public void EndNmi()
    {
        _sb.AppendLine($"{CurrentIndentation}rti");
        WriteEmptyLine();
    }

    public void StartMacro(string macroName, string[] paramNames)
    {
        _sb.AppendLine($"{CurrentIndentation}.macro {macroName} {string.Join(", ", paramNames)}");
        _indentation += 2;
    }

    public void EndMacro()
    {
        _indentation -= 2;
        _sb.AppendLine($"{CurrentIndentation}.endmacro");
        WriteEmptyLine();
    }

    public void WriteChars(int[] charBytes)
    {
        int i = 0;
        foreach (var charByte in charBytes)
        {
            _sb.AppendLine($"{CurrentIndentation}.byte {charByte}");

            if (++i % 8 == 0) WriteEmptyLine();
        }

        WriteEmptyLine();
    }

    public void WriteChars(byte[] charBytes)
    {
        int i = 0;
        foreach (var charByte in charBytes)
        {
            _sb.AppendLine($"{CurrentIndentation}.byte {charByte}");

            if (++i % 8 == 0) WriteEmptyLine();
        }

        WriteEmptyLine();
    }

    public void Write8CharBytes(byte[] charBytes)
    {
        if (charBytes.All(b => b == 0))
        {
            _sb.AppendLine($"{CurrentIndentation}.byte 0, 0, 0, 0, 0, 0, 0, 0");
        }
        else
        {
            foreach (var charByte in charBytes)
            {
                _sb.AppendLine($"{CurrentIndentation}.byte %{Convert.ToString(charByte, 2).PadLeft(8, '0')}");
            }
            WriteEmptyLine();
        }
    }

    public void WritePaletteColorsChars(byte[] charBytes)
    {
        _sb.AppendLine($"{CurrentIndentation}.byte {string.Join(", ", charBytes.Select(cb => $"${cb:X2}"))}");

        WriteEmptyLine();
    }

    public void WriteComment(string comment) => _sb.AppendLine($"{CurrentIndentation}; {comment}");
    
    public void WriteLabel(string label) => _sb.AppendLine($"{CurrentLabelIndentation}@{label}:");
    public void WriteVariableLabel(string variableLabel) => _sb.AppendLine($"{CurrentLabelIndentation}{variableLabel}:");
    public void WriteUnnamedLabel() => _sb.AppendLine($"{CurrentLabelIndentation}:");

    public void WriteConstant(string constName, string value) => _sb.AppendLine($"{CurrentIndentation}{constName} = {value}");

    public void WriteOpCode(string opCode) => _sb.AppendLine($"{CurrentIndentation}{opCode}");
    public void WriteOpCode(string opCode, byte address) => _sb.AppendLine($"{CurrentIndentation}{opCode} {address}");
    public void WriteOpCode(string opCode, string address) => _sb.AppendLine($"{CurrentIndentation}{opCode} {address}");
    public void WriteOpCode(string opCode, string baseAddress, string indexorRegister) => _sb.AppendLine($"{CurrentIndentation}{opCode} {baseAddress}, {indexorRegister}");

    public void WriteOpCodeImmediate(string opCode, byte value) => _sb.AppendLine($"{CurrentIndentation}{opCode} #{value}");
    public void WriteOpCodeImmediate(string opCode, string value) => _sb.AppendLine($"{CurrentIndentation}{opCode} #{value}");

    public void WriteJSROpCode(string? callingScope, string procName) => _sb.AppendLine($"{CurrentIndentation}jsr {GetFullName(callingScope, procName)}");
    public void WriteJMPOpCode(string? callingScope, string procName) => _sb.AppendLine($"{CurrentIndentation}jmp {GetFullName(callingScope, procName)}");
    public void WriteJMPToLabelOpCode(string label) => _sb.AppendLine($"{CurrentIndentation}jmp @{label}");

    public void WriteCallMacro(string? callingScope, string macroName, string[] operands)
        => _sb.AppendLine($"{CurrentIndentation}{GetFullName(callingScope, macroName)} {string.Join(", ", operands)}");

    public void WriteBranchOpCode(string opCode, string label)
    {
        var fullLabel = label.Contains("+") || label.Contains("-")
            ? $":{label}"
            : $"@{label}";

        _sb.AppendLine($"{CurrentIndentation}{opCode} {fullLabel}");
    }

    public override string ToString() => _sb.ToString();

    private string GetFullName(string? callingScope, string procName) => callingScope != null ? $"{callingScope}::{procName}" : procName;
}
