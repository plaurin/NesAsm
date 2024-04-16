﻿using BigGustave;
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

        // TODO Transparent color
        // TODO Split sprite tiles and background tiles + palettes
        // Generate palettes data
        colorPalettes.WriteData(writer);

        // NES only support 4 color palettes at a time, could be a problem if more
        if (colorPalettes.Count > 4)
        {
            // TODO only default color should use any other palette, don't create a palette only for that!
            context.ReportDiagnostic(CharDiagnostics.MoreThanFourColorPalettes, context.Location, colorPalettes.Count);
        }

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

            writer.Write8CharBytes(lowBytes.ToArray());
            writer.Write8CharBytes(highBytes.ToArray());
            writer.WriteEmptyLine();
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
                writer.WritePaletteColorsChars(palette.NesColors);
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

        internal static Pixel MatchNesColor(Pixel color, int x, int y, CharVisitorContext context)
        {
            int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B) + Math.Abs(color.A - otherColor.A);

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
