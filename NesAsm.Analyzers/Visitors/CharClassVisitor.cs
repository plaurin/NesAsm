using BigGustave;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NesAsm.Analyzers.Visitors;

public static class CharClassVisitor
{
    internal static void Visit(ClassDeclarationSyntax classDeclarationSyntax, VisitorContext context)
    {
        foreach (var att in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attribute in att.Attributes)
            {
                if (attribute.Name.ToString() == "ImportChar")
                {
                    var filepath = (attribute.ArgumentList!.Arguments[0].Expression as LiteralExpressionSyntax).Token.ValueText;

                    var sourceFilepath = Path.GetFullPath(classDeclarationSyntax.SyntaxTree.FilePath);
                    var sourceFolderPath = Path.GetDirectoryName(sourceFilepath);
                    var charFilepath = Path.Combine(sourceFolderPath, filepath);

                    var classNamespace = classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name?.ToString();
                    classNamespace ??= classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault()?.Name?.ToString();

                    var className = classDeclarationSyntax.Identifier.ToString();
                    var charVisitorContext = new CharVisitorContext(context, attribute.GetLocation());

                    Process(className, classNamespace, charFilepath, charVisitorContext);

                    return;
                }
            }
        }
    }

    internal static void Process(string className, string? classNamespace, string imageFilename, CharVisitorContext context)
    {
        var writer = context.Writer;

        var image = Png.Open(imageFilename);

        // Image should have specific size
        if (image.Width != 143 || image.Height != 287)
        {
            context.ReportDiagnostic(CharDiagnostics.ImageSizeNotValid, context.Location, image.Width, image.Height);
            return;
        }

        var (spriteTiles, spritePalettes) = GetTilesAndPalettes(image, 0, context);
        var (backgroundTiles, backgroundPalettes) = GetTilesAndPalettes(image, 256, context);

        writer.StartClassScope(className);

        OutputCharData(spriteTiles, backgroundTiles, spritePalettes, backgroundPalettes, context);

        writer.EndClassScope();

        // Warn if using colors that doesn't match exactly NES colors palette (from Mesen)
        if (context.MismatchColors.Any())
        {
            foreach (var (actualColor, expectedColor, x, y) in context.MismatchColors)
            {
                var actual = $"{actualColor.R}, {actualColor.G}, {actualColor.B}";
                var expected = $"{expectedColor.R}, {expectedColor.G}, {expectedColor.B}";
                context.ReportDiagnostic(CharDiagnostics.ColorMismatch, context.Location, actual, expected, x, y);
            }
        }

        // Now output the partial cs file with image data to be referenced in the scripts
        if (context.CsWriter == null)
        {
            context.ReportDiagnostic(CharDiagnostics.InternalAnalyzerFailure, context.Location, "CsWriter is null!");
        }
        else
        {
            // TODO Test when class is not partial
            OutputCharCSharp(className, classNamespace, spriteTiles, backgroundTiles, spritePalettes, backgroundPalettes, context);
        }
    }

    private static void OutputCharData(
        IEnumerable<TileData> spriteTiles,
        IEnumerable<TileData> backgroundTiles,
        ColorPalettes spritePalettes,
        ColorPalettes backgroundPalettes,
        CharVisitorContext context)
    {
        var writer = context.Writer;

        writer.StartCharsSegment();

        // Output tiles data
        foreach (var tile in spriteTiles)
        {
            writer.WriteComment($"Sprite Tile {tile.Index}");
            tile.WriteData(writer);
        }

        // If at least 1 background tile is not empty, need to output all sprite tiles even if empty
        if (backgroundTiles.Any())
        {
            writer.WriteComment($"Sprite Tiles {spriteTiles.Count()} to 255 are empty");
            for (var i = spriteTiles.Count(); i < 256; i++)
                writer.WriteLineWithIndentation(".byte 0, 0, 0, 0, 0, 0, 0, 0,  0, 0, 0, 0, 0, 0, 0, 0");
            
            writer.WriteEmptyLine();
        }

        foreach (var tile in backgroundTiles)
        {
            writer.WriteComment($"Background Tile {tile.Index}");
            tile.WriteData(writer);
        }

        // Output palettes data
        // TODO Transparent color
        // TODO First color always transparent 0F?
        writer.StartCodeSegment();

        if (backgroundPalettes.NonEmptyCount > 0)
        {
            writer.WriteVariableLabel($"background_palettes");
            backgroundPalettes.WriteAsm("Background", writer);
        }

        if (spritePalettes.NonEmptyCount > 0)
        {
            writer.WriteVariableLabel($"sprite_palettes");
            spritePalettes.WriteAsm("Sprite", writer);
        }
    }

    private static void OutputCharCSharp(
        string className,
        string? classNamespace,
        IEnumerable<TileData> spriteTiles,
        IEnumerable<TileData> backgroundTiles,
        ColorPalettes spritePalettes,
        ColorPalettes backgroundPalettes, 
        CharVisitorContext context)
    {
        var csWriter = context.CsWriter!;

        csWriter.WriteLineWithIndentation("using NesAsm.Emulator;");
        csWriter.WriteEmptyLine();

        if (classNamespace != null)
        {
            csWriter.WriteLineWithIndentation($"namespace {classNamespace};");
            csWriter.WriteEmptyLine();
        }

        csWriter.WriteLineWithIndentation($"public partial class {className} : CharDefinition");
        csWriter.WriteStartBlock();

        if (backgroundPalettes.NonEmptyCount > 0)
        {
            csWriter.WriteLineWithIndentation($"public static byte[] BackgroundPalettes = [");

            csWriter.IncreaseIndentation();
            backgroundPalettes.WriteCSharp(csWriter);
            csWriter.DecreaseIndentation();

            csWriter.WriteLineWithIndentation($"];");
        }

        if (spritePalettes.NonEmptyCount > 0)
        {
            csWriter.WriteLineWithIndentation($"public static byte[] SpritePalettes = [");

            csWriter.IncreaseIndentation();
            spritePalettes.WriteCSharp(csWriter);
            csWriter.DecreaseIndentation();

            csWriter.WriteLineWithIndentation($"];");
            csWriter.WriteEmptyLine();
        }

        csWriter.WriteEndBlock();
    }

    private static (IEnumerable<TileData> tiles, ColorPalettes palettes) GetTilesAndPalettes(Png image, int startingTileIndex, CharVisitorContext context)
    {
        var tiles = new List<TileData>();
        var palettes = new ColorPalettes();

        var lastTile = -1;

        // Generate 256 TileData
        for (int tileIndex = 0; tileIndex < 16 * 16; tileIndex++)
        {
            var tileData = CreateTileData(image, tileIndex + startingTileIndex, palettes, context);
            tiles.Add(tileData);

            if (!tileData.Palette.IsEmpty)
                lastTile = tileIndex;
        }

        // NES only support 4 non empty color palettes at a time, could be a problem if more
        if (palettes.NonEmptyCount > 4)
        {
            context.ReportDiagnostic(CharDiagnostics.MoreThanFourColorPalettes, context.Location, palettes.NonEmptyCount);
        }

        return (tiles.Take(lastTile + 1), palettes);
    }

    private static TileData CreateTileData(Png image, int tileIndex, ColorPalettes colorPalettes, CharVisitorContext context)
    {
        var tileStartX = (tileIndex % 16) * 9;
        var tileStartY = (tileIndex / 16) * 9;

        var pixels = new List<Pixel>();

        for (var j = 0; j < 8; j++)
            for (var i = 0; i < 8; i++)
            {
                var color = image.GetPixel(tileStartX + i, tileStartY + j);
                var nesColor = ColorPalette.MatchNesColor(color, tileStartX + i, tileStartY + j, context);
                pixels.Add(nesColor);
            }

        var colors = pixels.Distinct().ToArray();

        // Warning if more than 3 non default color
        if (colors.Length > 4)
        {
            context.ReportDiagnostic(CharDiagnostics.MoreThanThreeNonDefaultColorTile, context.Location, tileIndex, colors.Length);
            colors = colors.Take(4).ToArray();
        }

        var colorPalette = colorPalettes.GetFromColors(colors);

        return new TileData(pixels.ToArray(), colorPalette, tileIndex);
    }

    public class TileData
    {
        public TileData(Pixel[] pixels, ColorPalette palette, int index)
        {
            if (pixels.Length != 64)
                throw new ArgumentException("pixels must be 64 length", nameof(pixels));

            Pixels = pixels;
            Palette = palette;
            Index = index;
        }

        public Pixel[] Pixels { get; }

        public ColorPalette Palette { get; }
        
        public int Index { get; }

        internal void WriteData(AsmWriter writer)
        {
            var lowBytes = new List<byte>();
            var highBytes = new List<byte>();

            for (var j = 0; j < 8; j++)
            {
                byte lowByte = 0;
                byte highByte = 0;
                for (var i = 0; i < 8; i++)
                {
                    var color = Pixels[i + j * 8];

                    // TODO Fix B
                    byte colorIndex = Palette!.GetColorIndex(color);

                    if (colorIndex == 255)
                        throw new Exception("Color not in palette");

                    lowByte *= 2;
                    lowByte += (byte)(colorIndex % 2);

                    highByte *= 2;
                    highByte += (byte)(colorIndex / 2);
                }

                lowBytes.Add(lowByte);
                highBytes.Add(highByte);
            }

            writer.Write8CharBytes(lowBytes.ToArray());
            writer.Write8CharBytes(highBytes.ToArray());
            writer.WriteEmptyLine();
        }
    }

    public class ColorPalettes
    {
        private readonly HashSet<ColorPalette> palettes = new(new ColorPaletteComparer());

        public int NonEmptyCount => palettes.Count(p => !p.IsEmpty);

        public ColorPalette GetFromColors(Pixel[] colors)
        {
            var palette = new ColorPalette(colors);

            return GetFromPalette(palette);
        }

        public ColorPalette GetFromPalette(ColorPalette palette)
        {
            if (!palettes.Contains(palette))
            {
                palettes.Add(palette);
            }

            return palette;
        }

        internal void WriteAsm(string segment, AsmWriter writer)
        {
            var index = 0;
            foreach (var palette in palettes.Where(p => !p.IsEmpty))
            {
                writer.WriteComment($"{segment} Palette {index++}");
                writer.WritePaletteColorsChars(palette.NesColors);
            }
        }

        internal void WriteCSharp(CsWriter writer)
        {
            var index = 0;
            foreach (var palette in palettes.Where(p => !p.IsEmpty))
            {
                writer.WritePaletteColorsData(palette.NesColors);
                writer.WriteEndOfLineComment($"Palette {index++}");
            }
        }

        private class ColorPaletteComparer : IEqualityComparer<ColorPalette>
        {
            public bool Equals(ColorPalette x, ColorPalette y) => x.Colors.All(y.Colors.Contains);

            public int GetHashCode(ColorPalette obj) => obj.HashCode;
        }
    }

    public class ColorPalette
    {
        private static readonly Pixel[] AllNesColors = new Pixel[]
        {
            new(102, 102, 102),
            new(0, 42, 136),
            new(20, 18, 167),
            new(59, 0, 164),
            new(92, 0, 126),
            new(110, 0, 64),
            new(108, 6, 0),
            new(86, 29, 0),

            new(51, 53, 0),
            new(11, 72, 0),
            new(0, 82, 0),
            new(0, 79, 8),
            new(0, 64, 77),
            new(), // Never use!! https://www.nesdev.org/wiki/Color_$0D_games
            new(), // new(0, 0, 0),
            new(0, 0, 0),

            new(173, 173, 173),
            new(21, 95, 217),
            new(66, 64, 255),
            new(117, 39, 254),
            new(160, 26, 204),
            new(183, 30, 123),
            new(181, 49, 32),
            new(153, 78, 0),

            new(107, 109, 0),
            new(56, 135, 0),
            new(12, 147, 0),
            new(0, 143, 50),
            new(0, 124, 141),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),

            new(255, 255, 255), // new(255, 254, 255),
            new(100, 176, 255),
            new(146, 144, 255),
            new(198, 118, 255),
            new(243, 106, 255),
            new(254, 110, 204),
            new(254, 129, 112),
            new(234, 158, 34),

            new(188, 190, 0),
            new(136, 216, 0),
            new(92, 228, 48),
            new(69, 224, 130),
            new(72, 205, 222),
            new(79, 79, 79),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),

            new(), // new(255, 254, 255),
            new(192, 223, 255),
            new(211, 210, 255),
            new(232, 200, 255),
            new(251, 194, 255),
            new(254, 196, 234),
            new(254, 204, 197),
            new(247, 216, 165),

            new(228, 229, 148),
            new(207, 239, 150),
            new(189, 244, 171),
            new(179, 243, 204),
            new(181, 235, 242),
            new(184, 184, 184),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),
        };

        private readonly Pixel[] _colors = new Pixel[4];

        public ColorPalette(Pixel[] colors)
        {
            colors.CopyTo(_colors, 0);
        }

        public bool IsEmpty => _colors[0].R == 0 && _colors[0].G == 0 && _colors[0].B == 0 && _colors[1] == Pixel.Empty && _colors[2] == Pixel.Empty && _colors[3] == Pixel.Empty;

        public byte GetColorIndex(Pixel color)
        {
            int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B);

            var matchingColor = _colors.OrderBy(CalcDistance).First();
            return (byte)Array.IndexOf(_colors, matchingColor);
        }

        public int HashCode
        {
            get
            {
                unchecked
                {
                    return _colors[0].GetHashCode() + _colors[1].GetHashCode() + _colors[2].GetHashCode() + _colors[3].GetHashCode();
                }
            }
        }

        public Pixel[] Colors => _colors;

        public byte[] NesColors => _colors.Select(c => GetNesColorIndex(c)).ToArray();

        private static byte GetNesColorIndex(Pixel color)
        {
            if (color.IsEmpty) return 0x0F;

            for (byte i = 0; i < AllNesColors.Length; i++)
                if (color == AllNesColors[i]) return i;

            // Should Not happen
            return 255;
        }

        internal static Pixel MatchNesColor(Pixel color, int x, int y, CharVisitorContext context)
        {
            int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B);

            var closestColor = AllNesColors.OrderBy(CalcDistance).First();

            // Warn if not exactly matching color
            if (CalcDistance(closestColor) > 0)
            {
                context.RecordColorMismatch(closestColor, color, x, y);
            }

            return closestColor;
        }
    }
}
