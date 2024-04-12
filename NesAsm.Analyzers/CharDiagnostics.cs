using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers;

internal static class CharDiagnostics
{
    /// <summary>
    /// NA0000: 'Internal analyzer failure' {0}
    /// </summary>
    internal static DiagnosticDescriptor InternalAnalyzerFailure = new(
        "NA1000",
        "Internal analyzer failure",
        "{0}",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA1001: Png image size should be 143 by 287 but was {0} by {1}
    /// </summary>
    internal static DiagnosticDescriptor ImageSizeNotValid = new(
        "NA1001",
        "Image size not valid",
        "Png image size should be 143 by 287 but was {0} by {1}",
        "Todo",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// NA1002: Png image contains {0} different color palettes
    /// </summary>
    internal static DiagnosticDescriptor MoreThanFourColorPalettes = new(
        "NA1002",
        "More than four palettes",
        "Png image contains {0} different color palettes",
        "Todo",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    /// <summary>
    /// NA1003: Tile index {0} contains {1} non default colors (should be 3 maximum in a color palette), excess colors will be ignored
    /// </summary>
    internal static DiagnosticDescriptor MoreThanThreeNonDefaultColorTile = new(
        "NA1003",
        "More than three non default color in tile",
        "Tile index {0} contains {1} non default colors (should be 3 maximum in a color palette), excess colors will be ignored",
        "Todo",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// NA1004: Color RGB {0} is not a valid NES color (using Mesen color palette), use the closest color RGB {1} instead
    /// </summary>
    internal static DiagnosticDescriptor ColorMismatch = new(
        "NA1003",
        "Color mismatch",
        "Color RGB {0} is not a valid NES color (using Mesen color palette), use the closest color RGB {1} instead (position x: {2}, y: {3})",
        "Todo",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
