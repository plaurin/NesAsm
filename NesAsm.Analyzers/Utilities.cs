using System.Globalization;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NesAsm.Analyzers;

public static class Utilities
{
    public static string GetProcName(MethodDeclarationSyntax method) => GetProcName(method.Identifier.ValueText);
    public static string GetProcName(string methodName) => $"{char.ToLowerInvariant(methodName[0])}{methodName.Substring(1)}";
    public static string GetLabelName(string fieldName) => $"{char.ToLowerInvariant(fieldName[0])}{fieldName.Substring(1)}";
    public static string GetConstName(string constName) => constName;

    public static string ConvertOperandToNumericText(string operand)
    {
        if (string.IsNullOrWhiteSpace(operand)) return operand;

        operand = operand.Replace("PPUCTRL", "0x2000");
        operand = operand.Replace("PPUMASK", "0x2001");
        operand = operand.Replace("PPUSTATUS", "0x2002");
        operand = operand.Replace("PPUADDR", "0x2006");
        operand = operand.Replace("PPUDATA", "0x2007");

        if (operand.Length >= 2)
        {
            if (operand.Substring(0, 2) == "0x" && byte.TryParse(operand.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var _)) return $"${operand.Substring(2)}";
            if (operand.Substring(0, 2) == "0x" && ushort.TryParse(operand.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var _)) return $"${operand.Substring(2)}";

            if (operand.Substring(0, 2) == "0b")
            {
                var result = operand.Substring(2).Replace("_", "");
                if (result.Length <= 8) return $"%{result}";
                throw new InvalidOperationException($"Expected parameter of type byte for binary format but found {operand}");
            }
        }

        if (byte.TryParse(operand, out var _)) return operand;
        if (ushort.TryParse(operand, out var _)) return operand;

        throw new InvalidOperationException($"Expected parameter of type byte or ushort but found {operand}");
    }
}
