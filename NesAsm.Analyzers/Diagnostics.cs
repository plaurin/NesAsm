using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers;

internal static class Diagnostics
{
    /// <summary>
    /// NA0000: 'Internal analyzer failure' {0}
    /// </summary>
    internal static DiagnosticDescriptor InternalAnalyzerFailure = new DiagnosticDescriptor(
        "NA0000",
        "Internal analyzer failure",
        "{0}",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0001: Type {0} is not supported for method parameters
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedParameterType = new DiagnosticDescriptor(
        "NA0001",
        "Unsupported parameter type",
        "Type {0} is not supported for method parameters",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0002: "Operand {0} not supported for parameter (should be byte, ushort or bool)
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedParameterType2 = new DiagnosticDescriptor(
        "NA0002",
        "Unsupported parameter type",
        "Operand {0} not supported for parameter (should be byte, ushort or bool)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0003: OpCode {0} not supported to use parameters (should only be LDAa, LDXa or LDYa)
    /// </summary>
    internal static DiagnosticDescriptor InvalidOpCodeUsingParameters = new DiagnosticDescriptor(
        "NA0003",
        "Unsupported OpCode using parameters",
        "OpCode {0} not supported to use parameters (should only be LDAa, LDXa or LDYa)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0004: Parameter type {0} not supported (should be byte or bool)
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedUsingParameterType = new DiagnosticDescriptor(
        "NA0004",
        "Unsupported using parameter type",
        "Parameter type {0} not supported (should be byte or bool)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0005: Invalid format {0} for type {1}
    /// </summary>
    internal static DiagnosticDescriptor InvalidFormat = new DiagnosticDescriptor(
        "NA0005",
        "Invalid format while parsing",
        "Invalid format {0} for type {1}",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0006: Missing RomDataAttribute or CharDataAttribute on byte[] field
    /// </summary>
    internal static DiagnosticDescriptor MissingFieldDataType = new DiagnosticDescriptor(
        "NA0006",
        "RomDataAttribute and CharDataAttribute required",
        "Missing RomDataAttribute or CharDataAttribute on byte[] field",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0007: Only one of RomDataAttribute or CharDataAttribute can be used at the same time
    /// </summary>
    internal static DiagnosticDescriptor ManyFieldDataType = new DiagnosticDescriptor(
        "NA0007",
        "RomDataAttribute and CharDataAttribute used",
        "Only one of RomDataAttribute or CharDataAttribute can be used at the same time",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0008: Only one of RomDataAttribute or CharDataAttribute can be used at the same time
    /// </summary>
    internal static DiagnosticDescriptor InstructionNotSupported = new DiagnosticDescriptor(
        "NA0008",
        "Instruction not supported",
        "Instruction is not currently supported: {0}",
        "Todo",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0009: OpCode {0} not supported for Absolute Indexed addressing mode
    /// </summary>
    internal static DiagnosticDescriptor AbsoluteIndexedOpCodeNotSupported = new DiagnosticDescriptor(
        "NA0009",
        "Absolute Indexed OpCode not supported",
        "OpCode {0} not supported for Absolute Indexed addressing mode",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// "NA0010: Absolute Indexed register {0} not supported
    /// </summary>
    internal static DiagnosticDescriptor AbsoluteIndexedRegisterNotSupported = new DiagnosticDescriptor(
        "NA0010",
        "Absolute Indexed Register not supported",
        "Absolute Indexed register {0} not supported",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
