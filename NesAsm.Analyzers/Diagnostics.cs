using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers;

internal static class Diagnostics
{
    /// <summary>
    /// NA0000: 'Internal analyzer failure' {0}
    /// </summary>
    internal static DiagnosticDescriptor InternalAnalyzerFailure = new(
        "NA0000",
        "Internal analyzer failure",
        "{0}",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0001: Type {0} is not supported for method parameters
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedParameterType = new(
        "NA0001",
        "Unsupported parameter type",
        "Type {0} is not supported for method parameters",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0002: "Operand {0} not supported for parameter (should be byte, ushort or bool)
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedParameterType2 = new(
        "NA0002",
        "Unsupported parameter type",
        "Operand {0} not supported for parameter (should be byte, ushort or bool)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0003: OpCode {0} not supported to use parameters (should only be LDAa, LDXa or LDYa)
    /// </summary>
    internal static DiagnosticDescriptor InvalidOpCodeUsingParameters = new(
        "NA0003",
        "Unsupported OpCode using parameters",
        "OpCode {0} not supported to use parameters (should only be LDAa, LDXa or LDYa)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0004: Parameter type {0} not supported (should be byte or bool)
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedUsingParameterType = new(
        "NA0004",
        "Unsupported using parameter type",
        "Parameter type {0} not supported (should be byte or bool)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0005: Invalid format {0} for type {1}
    /// </summary>
    internal static DiagnosticDescriptor InvalidFormat = new(
        "NA0005",
        "Invalid format while parsing",
        "Invalid format {0} for type {1}",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0006: Missing RomDataAttribute or CharDataAttribute on byte[] field
    /// </summary>
    internal static DiagnosticDescriptor MissingFieldDataType = new(
        "NA0006",
        "RomDataAttribute and CharDataAttribute required",
        "Missing RomDataAttribute or CharDataAttribute on byte[] field",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0007: Only one of RomDataAttribute or CharDataAttribute can be used at the same time
    /// </summary>
    internal static DiagnosticDescriptor ManyFieldDataType = new(
        "NA0007",
        "RomDataAttribute and CharDataAttribute used",
        "Only one of RomDataAttribute or CharDataAttribute can be used at the same time",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0008: Instruction is not currently supported: {0}
    /// </summary>
    internal static DiagnosticDescriptor InstructionNotSupported = new(
        "NA0008",
        "Instruction not supported",
        "Instruction is not currently supported: {0}",
        "Todo",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0009: OpCode {0} not supported for Absolute Indexed addressing mode
    /// </summary>
    internal static DiagnosticDescriptor AbsoluteIndexedOpCodeNotSupported = new(
        "NA0009",
        "Absolute Indexed OpCode not supported",
        "OpCode {0} not supported for Absolute Indexed addressing mode",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0010: Absolute Indexed register {0} not supported
    /// </summary>
    internal static DiagnosticDescriptor AbsoluteIndexedRegisterNotSupported = new(
        "NA0010",
        "Absolute Indexed Register not supported",
        "Absolute Indexed register {0} not supported",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA0011: Branching OpCode {0} not supported
    /// </summary>
    internal static DiagnosticDescriptor BranchingOpCodeNotSupported = new(
        "NA0011",
        "Branching OpCode not supported",
        "Branching OpCode {0} not supported",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
