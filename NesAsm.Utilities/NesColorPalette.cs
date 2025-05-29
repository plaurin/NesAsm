using System;
using System.Diagnostics;
using System.Linq;
using BigGustave.PngUtil;

namespace NesAsm.Utilities
{
    public static class NesColorPalettesExtensions
    {
        public static NesColorPalette FindFromNesColors(this NesColorPalette[] palettes, byte[] nesColors)
        {
            foreach (var palette in palettes)
            {
                int i;
                for (i = 0; nesColors.Length > i; i++)
                {
                    if (!palette.NesColors.Contains(nesColors[i])) break;
                }

                if (i == nesColors.Length) return palette;
            }

            return null;
        }

        public static string ToHexDisplay(this byte[] nesColors) => string.Join(" ", nesColors.Select(c => $"{c:X2}"));

    }

    [DebuggerDisplay("{NesColorHex}")]
    public class NesColorPalette
    {
        public static readonly Pixel[] AllNesColors = new Pixel[]
        {
            // 0x00
            new Pixel(102, 102, 102),
            new Pixel(0, 42, 136),
            new Pixel(20, 18, 167),
            new Pixel(59, 0, 164),
            new Pixel(92, 0, 126),
            new Pixel(110, 0, 64),
            new Pixel(108, 6, 0),
            new Pixel(86, 29, 0),

            // 0x08
            new Pixel(51, 53, 0),
            new Pixel(11, 72, 0),
            new Pixel(0, 82, 0),
            new Pixel(0, 79, 8),
            new Pixel(0, 64, 77),
            new Pixel(), // Never use!! https://www.nesdev.org/wiki/Color_$0D_games
            new Pixel(), // new(0, 0, 0),
            new Pixel(0, 0, 0),

            // 0x10
            new Pixel(173, 173, 173),
            new Pixel(21, 95, 217),
            new Pixel(66, 64, 255),
            new Pixel(117, 39, 254),
            new Pixel(160, 26, 204),
            new Pixel(183, 30, 123),
            new Pixel(181, 49, 32),
            new Pixel(153, 78, 0),

            // 0x18
            new Pixel(107, 109, 0),
            new Pixel(56, 135, 0),
            new Pixel(12, 147, 0),
            new Pixel(0, 143, 50),
            new Pixel(0, 124, 141),
            new Pixel(), // new(0, 0, 0),
            new Pixel(), // new(0, 0, 0),
            new Pixel(), // new(0, 0, 0),

            // 0x20
            new Pixel(255, 255, 255), // new(255, 254, 255),
            new Pixel(100, 176, 255),
            new Pixel(146, 144, 255),
            new Pixel(198, 118, 255),
            new Pixel(243, 106, 255),
            new Pixel(254, 110, 204),
            new Pixel(254, 129, 112),
            new Pixel(234, 158, 34),

            // 0x28
            new Pixel(188, 190, 0),
            new Pixel(136, 216, 0),
            new Pixel(92, 228, 48),
            new Pixel(69, 224, 130),
            new Pixel(72, 205, 222),
            new Pixel(79, 79, 79),
            new Pixel(), // new(0, 0, 0),
            new Pixel(), // new(0, 0, 0),

            // 0x30
            new Pixel(255, 254, 255),
            new Pixel(192, 223, 255),
            new Pixel(211, 210, 255),
            new Pixel(232, 200, 255),
            new Pixel(251, 194, 255),
            new Pixel(254, 196, 234),
            new Pixel(254, 204, 197),
            new Pixel(247, 216, 165),

            // 0x38
            new Pixel(228, 229, 148),
            new Pixel(207, 239, 150),
            new Pixel(189, 244, 171),
            new Pixel(179, 243, 204),
            new Pixel(181, 235, 242),
            new Pixel(184, 184, 184),
            new Pixel(), // new(0, 0, 0),
            new Pixel(), // new(0, 0, 0),
        };

        private readonly byte[] _nesColors = new byte[4];

        public NesColorPalette(byte[] nesColors)
        {
            nesColors.CopyTo(_nesColors, 0);
        }

        public byte[] NesColors => _nesColors;

        public string NesColorHex => NesColors.ToHexDisplay();

        internal static byte MatchNesColor(Pixel color, int x, int y)
        {
            int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B);

            var closestColor = AllNesColors.OrderBy(CalcDistance).First();

            // Warn if not exactly matching color
            if (CalcDistance(closestColor) > 0)
            {
                Console.WriteLine("Color mismatch for color {0}, closest is {1} at pixel {2}, {3}", closestColor, color, x, y);
            }

            return (byte)AllNesColors.ToList().IndexOf(closestColor);
        }

        internal byte GetColorIndex(byte nesColor)
        {
            if (_nesColors[0] == nesColor) return 0;
            if (_nesColors[1] == nesColor) return 1;
            if (_nesColors[2] == nesColor) return 2;
            if (_nesColors[3] == nesColor) return 3;
            return 0;
        }
    }
}
