using Microsoft.CodeAnalysis.CSharp.Syntax;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NesAsm.Analyzers.Visitors;

public static class CharClassVisitor
{
    public static void Visit(ClassDeclarationSyntax classDeclarationSyntax)
    {
    }

    public static string Process(string filename)
    {
        var writer = new AsmWriter();
        var bitmap = SKBitmap.Decode(filename);

        // Error if not 287x287

        var tiles = new List<TileData>();
        var colorPalettes = new ColorPalettes();

        var lastTile = 0;

        // Generate palette for each tile
        for (int tileIndex = 0; tileIndex < 16 * 16; tileIndex++)
        {
            var tileData = CreateTileData(bitmap, tileIndex, colorPalettes);
            tiles.Add(tileData);

            if (!tileData.Palette.IsEmpty)
                lastTile = tileIndex;
        }

        // Warning if more than 4 palettes?

        // Generate tiles data
        for (int tileIndex = 0; tileIndex <= lastTile; tileIndex++)
        {
            writer.WriteComment($"Tile {tileIndex}");
            tiles[tileIndex].WriteData(writer);
        }

        // Generate palettes data
        colorPalettes.WriteData(writer);

        return writer.ToString();
    }

    private static TileData CreateTileData(SKBitmap bitmap, int tileIndex, ColorPalettes colorPalettes)
    {
        var tileStartX = (tileIndex % 16) * 9;
        var tileStartY = (tileIndex / 16) * 9;

        var pixels = new List<SKColor>();

        for (var j = 0; j < 8; j++)
            for (var i = 0; i < 8; i++)
            {
                var color = bitmap.GetPixel(tileStartX + i, tileStartY + j);
                var nesColor = ColorPalette.MatchNesColor(color);
                pixels.Add(nesColor);
            }

        // Error if more than 3 non default color

        var colors = pixels.Distinct().ToArray();
        var colorPalette = colorPalettes.GetFromColors(colors);

        return new TileData(pixels.ToArray(), colorPalette);
    }

    public class TileData
    {
        public TileData(SKColor[] pixels, ColorPalette palette)
        {
            if (pixels.Length != 64)
                throw new ArgumentException("pixels must be 64 length", nameof(pixels));

            Pixels = pixels;
            Palette = palette;
        }

        public SKColor[] Pixels { get; }

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

        public ColorPalette GetFromColors(SKColor[] colors)
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
        private static readonly SKColor[] AllNesColors = new SKColor[]
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
            SKColor.Empty,
            SKColor.Empty, // new(0, 0, 0),
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
            SKColor.Empty, // new(0, 0, 0),
            SKColor.Empty, // new(0, 0, 0),
            SKColor.Empty, // new(0, 0, 0),

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
            SKColor.Empty, // new(0, 0, 0),
            SKColor.Empty, // new(0, 0, 0),

            SKColor.Empty, // new(255, 255, 255),
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
            SKColor.Empty, // new(0, 0, 0),
            SKColor.Empty, // new(0, 0, 0),
        };

        private readonly SKColor[] _colors = new SKColor[4];

        public ColorPalette(SKColor[] colors)
        {
            colors.CopyTo(_colors, 0);
        }

        public bool IsEmpty => _colors[0].Red == 0 && _colors[0].Green == 0 && _colors[0].Blue == 0 && _colors[1] == SKColor.Empty && _colors[2] == SKColor.Empty && _colors[3] == SKColor.Empty;

        public byte GetColorIndex(SKColor color)
        {
            int CalcDistance(SKColor otherColor) => Math.Abs(color.Red - otherColor.Red) + Math.Abs(color.Green - otherColor.Green) + Math.Abs(color.Blue - otherColor.Blue) + Math.Abs(color.Alpha - otherColor.Alpha);

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

        public SKColor[] Colors => _colors;

        public byte[] NesColors => _colors.Select(c => GetNesColorIndex(c)).ToArray();

        private static byte GetNesColorIndex(SKColor color)
        {
            for (byte i = 0; i < AllNesColors.Length; i++)
                if (color == AllNesColors[i]) return i;

            // Should Not happen
            return 255;
        }

        public static SKColor MatchNesColor(SKColor color)
        {
            int CalcDistance(SKColor otherColor) => Math.Abs(color.Red - otherColor.Red) + Math.Abs(color.Green - otherColor.Green) + Math.Abs(color.Blue - otherColor.Blue) + Math.Abs(color.Alpha - otherColor.Alpha);

            var closestColor = AllNesColors.OrderBy(CalcDistance).First();

            if (CalcDistance(closestColor) > 0)
            {
                // Warn if not exactly matching color
            }

            return closestColor;
        }
    }
}
