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
    /// NA0002: "Operand {0} not supported for parameter (should be byte, ushort or bool)"
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
    /// "NA0004: Parameter type {0} not supported (should be byte or bool)"
    /// </summary>
    internal static DiagnosticDescriptor UnsupportedUsingParameterType = new DiagnosticDescriptor(
        "NA0004",
        "Unsupported using parameter type",
        "Parameter type {0} not supported (should be byte or bool)",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
