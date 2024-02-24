using Microsoft.CodeAnalysis.CSharp.Syntax;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        //var colorPalettes = new HashSet<ColorPalette>(new ColorPaletteComparer());
        var colorPalettes = new List<ColorPalette>();

        var tilesPalette = new int[16, 16];

        // Generate palette for each tile
        for (var tileIndexY = 0; tileIndexY < 16; tileIndexY++)
            for (var tileIndexX = 0; tileIndexX < 16; tileIndexX++)
            {
                var colors = GetColors(bitmap, tileIndexX, tileIndexY);
                //if (!colorPalettes.Contains(colors))
                //    colorPalettes.Add(colors);

                var paletteIndex = colorPalettes.IndexOf(colors);
                if (paletteIndex == -1)
                {
                    paletteIndex = colorPalettes.Count;
                    colorPalettes.Add(colors);
                }

                tilesPalette[tileIndexX, tileIndexY] = paletteIndex;
            }

        // Warning if more than 4 palettes?

        // Create Tile data using PaletteColor
        for (var tileIndexY = 0; tileIndexY < 16; tileIndexY++)
            for (var tileIndexX = 0; tileIndexX < 16; tileIndexX++)
            {
                WriteTileData(bitmap, tileIndexX, tileIndexY, colorPalettes[tilesPalette[tileIndexX, tileIndexY]], writer);
            }

        return writer.ToString();
    }

    private static void WriteTileData(SKBitmap bitmap, int tileIndexX, int tileIndexY, ColorPalette colorPalette, AsmWriter writer)
    {
        var tileStartX = tileIndexX * 9;
        var tileStartY = tileIndexY * 9;

        var pixelData = new int[8, 8];

        for (var j = 0; j < 8; j++)
        {
            for (var i = 0; i < 8; i++)
            {
                var color = bitmap.GetPixel(tileStartX + i, tileStartY + j);

                var colorIndex = colorPalette.GetColorIndex(color);

                if (colorIndex == -1)
                    throw new Exception("Color not in palette");

                pixelData[i, j] = colorIndex;
            }
        }

        var values = new List<int>();

        for (var j = 0; j < 8; j++)
        {
            var total = 0;
            for (var i = 0; i < 8; i++)
            {
                total *= 2;
                total += pixelData[i, j] % 2;
            }

            values.Add(total);
        }

        for (var j = 0; j < 8; j++)
        {
            var total = 0;
            for (var i = 0; i < 8; i++)
            {
                total *= 2;
                total += pixelData[i, j] / 2;
            }

            values.Add(total);
        }

        writer.WriteChars(values.ToArray());

    }

    private static ColorPalette GetColors(SKBitmap bitmap, int tileIndexX, int tileIndexY)
    {
        var colors = new HashSet<SKColor>
        {
            new(0, 0, 0)
        };

        var tileStartX = tileIndexX * 9;
        var tileStartY = tileIndexY * 9;

        for (var j = 0; j < 8; j++)
            for (var i = 0; i < 8; i++)
            {
                var color = bitmap.GetPixel(tileStartX + i, tileStartY + j);

                var nesColor = MatchNesColor(color);

                if (!colors.Contains(nesColor))
                    colors.Add(nesColor);
            }

        // Error if more than 3 non default color

        return new ColorPalette(colors.ToArray());
    }

    private static SKColor MatchNesColor(SKColor color)
    {
        var nesColor = new SKColor[]
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

        int CalcDistance(SKColor otherColor) => Math.Abs(color.Red - otherColor.Red) + Math.Abs(color.Green - otherColor.Green) + Math.Abs(color.Blue - otherColor.Blue) + Math.Abs(color.Alpha - otherColor.Alpha);

        var closestColor = nesColor[0];
        var closestDistance = CalcDistance(closestColor);
        for (int i = 1; i < nesColor.Length; i++)
        {
            var distance = CalcDistance(nesColor[i]);
            if (distance < closestDistance)
            {
                closestColor = nesColor[i];
                closestDistance = distance;
            }
        }

        if (closestDistance > 0)
        {
            // Warn if not exactly matching color
        }

        return closestColor;
    }

    public class ColorPalette
    {
        public ColorPalette(SKColor color1, SKColor color2, SKColor color3, SKColor color4)
        {
            Color1 = color1;
            Color2 = color2;
            Color3 = color3;
            Color4 = color4;
        }

        public ColorPalette(SKColor[] colors)
        {
            if (colors.Length > 0) Color1 = colors[0];
            if (colors.Length > 1) Color2 = colors[1];
            if (colors.Length > 2) Color3 = colors[2];
            if (colors.Length > 3) Color4 = colors[3];
        }

        public int GetColorIndex(SKColor color)
        {
            int CalcDistance(SKColor otherColor) => Math.Abs(color.Red - otherColor.Red) + Math.Abs(color.Green - otherColor.Green) + Math.Abs(color.Blue - otherColor.Blue) + Math.Abs(color.Alpha - otherColor.Alpha);

            var closestColorIndex = 0;
            var closestDistance = CalcDistance(Color1);

            var distance = CalcDistance(Color2);
            if (distance < closestDistance)
            {
                closestColorIndex = 1;
                closestDistance = distance;
            }

            distance = CalcDistance(Color3);
            if (distance < closestDistance)
            {
                closestColorIndex = 2;
                closestDistance = distance;
            }

            distance = CalcDistance(Color4);
            if (distance < closestDistance)
            {
                closestColorIndex = 3;
            }

            return closestColorIndex;
        }

        public SKColor Color1 { get; }
        public SKColor Color2 { get; }
        public SKColor Color3 { get; }
        public SKColor Color4 { get; }
    }

    public class ColorPaletteComparer : IEqualityComparer<ColorPalette>
    {
        public bool Equals(ColorPalette x, ColorPalette y)
        {
            var leftColors = new[] { x.Color1, x.Color2, x.Color3 };
            var rightColors = new[] { y.Color1, y.Color2, y.Color3 };

            return leftColors.All(rightColors.Contains);
        }

        public int GetHashCode(ColorPalette obj)
        {
            unchecked
            {
                return obj.Color1.GetHashCode() + obj.Color2.GetHashCode() + obj.Color3.GetHashCode();
            }
        }
    }
}
