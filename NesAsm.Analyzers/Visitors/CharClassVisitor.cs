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

                    var sourceFilePath = Path.GetFullPath(classDeclarationSyntax.SyntaxTree.FilePath);
                    var sourceFolderPath = Path.GetDirectoryName(sourceFilePath);
                    var charFilePath = Path.Combine(sourceFolderPath, filepath);

                    var charVisitorContext = new CharVisitorContext(context, attribute.GetLocation());

                    Process(charFilePath, charVisitorContext);

                    return;
                }
            }
        }
    }

    internal static string Process(string filename, VisitorContext context)
    {
        var writer = new AsmWriter();

        var charVisitorContext = new CharVisitorContext(context, Location.None);

        Process(filename, charVisitorContext);

        return writer.ToString();
    }

    internal static void Process(string filename, CharVisitorContext context)
    {
        var writer = context.Writer;

        var image = Png.Open(filename);

        // Image should have specific size
        if (image.Width != 143 || image.Height != 287)
        {
            context.ReportDiagnostic(CharDiagnostics.ImageSizeNotValid, context.Location, image.Width, image.Height);
            return;
        }

        var tiles = new List<TileData>();
        var colorPalettes = new ColorPalettes();

        var lastTile = 0;

        // Generate palette for each tile
        for (int tileIndex = 0; tileIndex < 16 * 16; tileIndex++)
        {
            var tileData = CreateTileData(image, tileIndex, colorPalettes, context);
            tiles.Add(tileData);

            if (!tileData.Palette.IsEmpty)
                lastTile = tileIndex;
        }

        // Generate tiles data
        for (int tileIndex = 0; tileIndex <= lastTile; tileIndex++)
        {
            writer.WriteComment($"Tile {tileIndex}");
            tiles[tileIndex].WriteData(writer);
        }

        // Generate palettes data
        colorPalettes.WriteData(writer);

        // NES only support 4 color palettes at a time, could be a problem if more
        if (colorPalettes.Count > 4)
        {
            context.ReportDiagnostic(CharDiagnostics.MoreThanFourColorPalettes, context.Location, colorPalettes.Count);
        }
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
                var nesColor = ColorPalette.MatchNesColor(color);
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

        return new TileData(pixels.ToArray(), colorPalette);
    }

    public class TileData
    {
        public TileData(Pixel[] pixels, ColorPalette palette)
        {
            if (pixels.Length != 64)
                throw new ArgumentException("pixels must be 64 length", nameof(pixels));

            Pixels = pixels;
            Palette = palette;
        }

        public Pixel[] Pixels { get; }

        public ColorPalette Palette { get; }

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

            writer.WriteChars(lowBytes.ToArray());
            writer.WriteChars(highBytes.ToArray());
        }
    }

    public class ColorPalettes
    {
        private readonly HashSet<ColorPalette> palettes = new(new ColorPaletteComparer());

        public int Count => palettes.Count;

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

        internal void WriteData(AsmWriter writer)
        {
            var index = 0;
            foreach (var palette in palettes)
            {
                writer.WriteComment($"Palette {index++}");
                for (var i = 0; i < palette.Colors.Length; i++)
                {
                    writer.WriteChars(palette.NesColors);
                }
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
            new(98, 98, 98),
            new(0, 31, 178),
            new(36, 4, 200),
            new(82, 0, 178),
            new(115, 0, 118),
            new(128, 0, 36),
            new(115, 11, 0),
            new(82, 40, 0),
            new(36, 68, 0),
            new(0, 87, 0),
            new(0, 92, 0),
            new(0, 83, 36),
            new(0, 60, 118),
            new(),
            new(), // new(0, 0, 0),
            new(0, 0, 0),

            new(171, 171, 171),
            new(13, 87, 255),
            new(75, 48, 255),
            new(138, 19, 255),
            new(188, 8, 214),
            new(210, 18, 105),
            new(199, 46, 0),
            new(157, 84, 0),
            new(96, 124, 0),
            new(32, 152, 0),
            new(0, 163, 0),
            new(0, 153, 66),
            new(0, 125, 180),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),

            new(255, 255, 255),
            new(83, 174, 255),
            new(144, 133, 255),
            new(211, 101, 255),
            new(255, 87, 255),
            new(255, 93, 207),
            new(255, 119, 87),
            new(250, 158, 0),
            new(189, 199, 0),
            new(122, 231, 0),
            new(67, 246, 17),
            new(38, 239, 126),
            new(44, 213, 246),
            new(78, 78, 78),
            new(), // new(0, 0, 0),
            new(), // new(0, 0, 0),

            new(), // new(255, 255, 255),
            new(182, 225, 255),
            new(206, 209, 255),
            new(233, 195, 255),
            new(255, 188, 255),
            new(255, 189, 244),
            new(255, 198, 195),
            new(255, 213, 154),
            new(233, 230, 129),
            new(206, 244, 129),
            new(182, 251, 154),
            new(169, 250, 195),
            new(169, 240, 244),
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
            int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B) + Math.Abs(color.A - otherColor.A);

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
            for (byte i = 0; i < AllNesColors.Length; i++)
                if (color == AllNesColors[i]) return i;

            // Should Not happen
            return 255;
        }

        public static Pixel MatchNesColor(Pixel color)
        {
            int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B) + Math.Abs(color.A - otherColor.A);

            var closestColor = AllNesColors.OrderBy(CalcDistance).First();

            if (CalcDistance(closestColor) > 0)
            {
                // Warn if not exactly matching color
            }

            return closestColor;
        }
    }
}
