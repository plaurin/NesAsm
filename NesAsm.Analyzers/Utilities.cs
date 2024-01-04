using System.Globalization;
using System;

namespace NesAsm.Analyzers;

public static class Utilities
{
    public static string ConvertOperandToNumericText(string operand)
    {
        if (string.IsNullOrWhiteSpace(operand)) return operand;

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
