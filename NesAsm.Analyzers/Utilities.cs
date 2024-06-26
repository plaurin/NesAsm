﻿using System.Globalization;
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NesAsm.Analyzers;

public static class Utilities
{
    public static string GetProcName(MethodDeclarationSyntax method) => GetProcName(method.Identifier.ValueText);

    public static string GetProcName(string methodName) => $"{char.ToLowerInvariant(methodName[0])}{methodName.Substring(1)}";

    public static string GetLabelName(string fieldName)
    {
        var parts = fieldName.Split('.');
        parts[parts.Length - 1] = ToSnakeCase(parts[parts.Length - 1]);

        return string.Join("::", parts);
    }

    public static string GetConstName(string constName) => constName;

    public static string GetMacroName(MethodDeclarationSyntax method) => GetMacroName(method.Identifier.ValueText);

    public static string GetMacroName(string macroName) => macroName;


    public static string ConvertOperandToNumericText(string operand)
    {
        if (string.IsNullOrWhiteSpace(operand)) return operand;

        operand = operand.Replace("PPUCTRL", "0x2000");
        operand = operand.Replace("PPUMASK", "0x2001");
        operand = operand.Replace("PPUSTATUS", "0x2002");
        operand = operand.Replace("PPUADDR", "0x2006");
        operand = operand.Replace("PPUDATA", "0x2007");

        if (operand.EndsWith("/ 256")) return $"#.HIBYTE({operand.Replace("/ 256", "").Trim()})";
        if (operand.EndsWith("% 256")) return $"#.LOBYTE({operand.Replace("% 256", "").Trim()})";

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

        if (bool.TryParse(operand, out var boolOperand)) return boolOperand ? "1" : "0";
        if (byte.TryParse(operand, out var _)) return operand;
        if (ushort.TryParse(operand, out var _)) return operand;

        throw new InvalidOperationException($"Expected parameter of type bool, byte or ushort but found {operand}");
    }

    // https://stackoverflow.com/questions/63055621/how-to-convert-camel-case-to-snake-case-with-two-capitals-next-to-each-other
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var sb = new StringBuilder();
        sb.Append(char.ToLower(input[0]));

        for (var i = 1; i < input.Length; i++)
        {
            var currentChar = input[i];
            var previousChar = input[i - 1];

            if (char.IsUpper(currentChar))
            {
                if (previousChar != ' ' && !char.IsUpper(previousChar) && previousChar != '_')
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLower(currentChar));
            }
            else
            {
                sb.Append(currentChar);
            }
        }

        return sb.ToString();
    }

    public static string GetOutputFolder(string currentFolder, string nodeFilePath, string? outputPath)
    {
        outputPath ??= string.Empty;

        if (Path.IsPathRooted(nodeFilePath))
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(nodeFilePath)!, outputPath));
        }
        else
        {
            var nodeFolder = Path.GetDirectoryName(nodeFilePath);
            var upPath = string.Join("\\", nodeFolder.Split('\\').TakeWhile(x => x == ".."));
            var subPath = (nodeFolder != "" && upPath != "") ? nodeFolder.Replace(upPath, string.Empty).TrimStart('\\').TrimEnd('\\') : nodeFolder;

            var result = Path.GetFullPath(Path.Combine(currentFolder, upPath, outputPath, subPath));
            return result;
        }
    }
}
