using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BigGustave.PngUtil;

namespace NesAsm.Utilities
{
    public static class CharHelpers
    {
        public struct CharData
        {
            public TileData[] SpriteTiles;
            public ColorPalettes SpritePalettes;
            public TileData[] BackgroundTiles;
            public ColorPalettes BackgroundPalettes;
        }

        public static CharData? LoadImage(string imageFilename, bool hasTileSeparator = true, ColorPalettes backgroundPalettes = null, ColorPalettes spritePalettes = null)
        {
            var image = Png.Open(imageFilename);

            // Image should have specific size
            if (hasTileSeparator && (image.Width != 143 || image.Height != 287))
            {
                Console.WriteLine("Image size not valid width: {0} height: {1}", image.Width, image.Height);
                return null;
            }

            if (!hasTileSeparator && (image.Width != 128 || image.Height != 256))
            {
                Console.WriteLine("Image size not valid width: {0} height: {1}", image.Width, image.Height);
                return null;
            }

            var (spriteTiles, outSpritePalettes) = GetTilesAndPalettes(image, 0, hasTileSeparator, spritePalettes);
            var (backgroundTiles, outBackgroundPalettes) = GetTilesAndPalettes(image, 256, hasTileSeparator, backgroundPalettes);

            return new CharData
            {
                SpriteTiles = spriteTiles.ToArray(),
                SpritePalettes = outSpritePalettes,
                BackgroundTiles = backgroundTiles.ToArray(),
                BackgroundPalettes = outBackgroundPalettes
            };
        }

        private static (IEnumerable<TileData> tiles, ColorPalettes palettes) GetTilesAndPalettes(Png image, int startingTileIndex, bool hasTileSeparator, ColorPalettes existingPallettes = null)
        {
            var tiles = new TileData[256];
            var palettes = existingPallettes ?? new ColorPalettes();

            var lastTile = -1;

            // Generate 256 TileData
            for (int tileIndex = 0; tileIndex < 16 * 16; tileIndex++)
            {
                if (tiles[tileIndex] == null)
                {
                    var tileData = CreateTileData(image, tileIndex + startingTileIndex, palettes, hasTileSeparator);
                    tiles[tileIndex] = tileData;
                }

                if (!tiles[tileIndex].Palette.IsEmpty)
                    lastTile = tileIndex;
            }

            // NES only support 4 non empty color palettes at a time, could be a problem if more
            if (palettes.NonEmptyCount > 4)
            {
                Console.WriteLine("More than four color palettes found, count is {0}", palettes.NonEmptyCount);
            }

            return (tiles.Take(lastTile + 1), palettes);
        }

        private static TileData CreateTileData(Png image, int tileIndex, ColorPalettes colorPalettes, bool hasTileSeparator)
        {
            var tileSize = hasTileSeparator ? 9 : 8;

            var tileStartX = (tileIndex % 16) * tileSize;
            var tileStartY = (tileIndex / 16) * tileSize;

            var pixels = new List<Pixel>();

            // TODO Work with NESColor directly in ColorPalette

            for (var j = 0; j < 8; j++)
                for (var i = 0; i < 8; i++)
                {
                    var color = image.GetPixel(tileStartX + i, tileStartY + j);
                    var nesColor = ColorPalette.MatchNesColor(color, tileStartX + i, tileStartY + j);
                    pixels.Add(nesColor);
                }

            var colors = new[] { Pixel.Empty }.Concat(pixels).Distinct().ToArray();

            // Warning if more than 3 non default color
            if (colors.Length > 4)
            {
                Console.WriteLine("More than three non default color found for tile {0}, number of distinct color {1}", tileIndex, colors.Length);
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
        }

        public class ColorPalettes : IEnumerable<ColorPalette>
        {
            private readonly HashSet<ColorPalette> _palettes = new HashSet<ColorPalette>(new ColorPaletteComparer());

            public int NonEmptyCount => _palettes.Count(p => !p.IsEmpty);

            public IEnumerator<ColorPalette> GetEnumerator() => _palettes.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _palettes.GetEnumerator();

            public void AddNesColorPalette(byte[] nesColors)
            {
                if (nesColors == null) throw new ArgumentNullException(nameof(nesColors));
                if (nesColors.Length != 4) throw new ArgumentOutOfRangeException(nameof(nesColors), "Exactly 4 bytes required");

                var palette = new ColorPalette(new Pixel[]
                {
                    ColorPalette.AllNesColors[nesColors[0]],
                    ColorPalette.AllNesColors[nesColors[1]],
                    ColorPalette.AllNesColors[nesColors[2]],
                    ColorPalette.AllNesColors[nesColors[3]]
                });

                _palettes.Add(palette);
            }

            public ColorPalette GetFromColors(Pixel[] colors)
            {
                var palette = new ColorPalette(colors);

                return GetFromPalette(palette);
            }

            public ColorPalette GetFromPalette(ColorPalette palette)
            {
                if (!_palettes.Contains(palette))
                {
                    _palettes.Add(palette);
                }

                return palette;
            }

            private class ColorPaletteComparer : IEqualityComparer<ColorPalette>
            {
                public bool Equals(ColorPalette x, ColorPalette y) => x.Colors.All(y.Colors.Contains);

                public int GetHashCode(ColorPalette obj) => obj.HashCode;
            }
        }

        public class ColorPalette
        {
            public static readonly Pixel[] AllNesColors = new Pixel[]
            {
                new Pixel(102, 102, 102),
                new Pixel(0, 42, 136),
                new Pixel(20, 18, 167),
                new Pixel(59, 0, 164),
                new Pixel(92, 0, 126),
                new Pixel(110, 0, 64),
                new Pixel(108, 6, 0),
                new Pixel(86, 29, 0),

                new Pixel(51, 53, 0),
                new Pixel(11, 72, 0),
                new Pixel(0, 82, 0),
                new Pixel(0, 79, 8),
                new Pixel(0, 64, 77),
                new Pixel(), // Never use!! https://www.nesdev.org/wiki/Color_$0D_games
                new Pixel(), // new(0, 0, 0),
                new Pixel(0, 0, 0),

                new Pixel(173, 173, 173),
                new Pixel(21, 95, 217),
                new Pixel(66, 64, 255),
                new Pixel(117, 39, 254),
                new Pixel(160, 26, 204),
                new Pixel(183, 30, 123),
                new Pixel(181, 49, 32),
                new Pixel(153, 78, 0),

                new Pixel(107, 109, 0),
                new Pixel(56, 135, 0),
                new Pixel(12, 147, 0),
                new Pixel(0, 143, 50),
                new Pixel(0, 124, 141),
                new Pixel(), // new(0, 0, 0),
                new Pixel(), // new(0, 0, 0),
                new Pixel(), // new(0, 0, 0),

                new Pixel(255, 255, 255), // new(255, 254, 255),
                new Pixel(100, 176, 255),
                new Pixel(146, 144, 255),
                new Pixel(198, 118, 255),
                new Pixel(243, 106, 255),
                new Pixel(254, 110, 204),
                new Pixel(254, 129, 112),
                new Pixel(234, 158, 34),

                new Pixel(188, 190, 0),
                new Pixel(136, 216, 0),
                new Pixel(92, 228, 48),
                new Pixel(69, 224, 130),
                new Pixel(72, 205, 222),
                new Pixel(79, 79, 79),
                new Pixel(), // new(0, 0, 0),
                new Pixel(), // new(0, 0, 0),

                new Pixel(), // new(255, 254, 255),
                new Pixel(192, 223, 255),
                new Pixel(211, 210, 255),
                new Pixel(232, 200, 255),
                new Pixel(251, 194, 255),
                new Pixel(254, 196, 234),
                new Pixel(254, 204, 197),
                new Pixel(247, 216, 165),

                new Pixel(228, 229, 148),
                new Pixel(207, 239, 150),
                new Pixel(189, 244, 171),
                new Pixel(179, 243, 204),
                new Pixel(181, 235, 242),
                new Pixel(184, 184, 184),
                new Pixel(), // new(0, 0, 0),
                new Pixel(), // new(0, 0, 0),
            };

            private readonly Pixel[] _colors = new Pixel[4];
            private readonly byte[] _nesColors = new byte[4];

            public ColorPalette(Pixel[] colors)
            {
                colors.CopyTo(_colors, 0);
            }

            public ColorPalette(byte[] nesColors)
            {
                nesColors.CopyTo(_nesColors, 0);
            }

            public bool IsEmpty => _colors[0] == Pixel.Empty && _colors[1] == Pixel.Empty && _colors[2] == Pixel.Empty && _colors[3] == Pixel.Empty;

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

            public string Name { get; private set; }

            private static byte GetNesColorIndex(Pixel color)
            {
                if (color.IsEmpty) return 0x0F;

                for (byte i = 0; i < AllNesColors.Length; i++)
                    if (color == AllNesColors[i]) return i;

                // Should Not happen
                return 255;
            }

            internal static Pixel MatchNesColor(Pixel color, int x, int y)
            {
                int CalcDistance(Pixel otherColor) => Math.Abs(color.R - otherColor.R) + Math.Abs(color.G - otherColor.G) + Math.Abs(color.B - otherColor.B);

                var closestColor = AllNesColors.OrderBy(CalcDistance).First();

                // Warn if not exactly matching color
                if (CalcDistance(closestColor) > 0)
                {
                    Console.WriteLine("Color mismatch for color {0}, closest is {1} at pixel {2}, {3}", closestColor, color, x, y);
                }

                return AllNesColors.IndexOf(closestColor);
            }

            internal void SetName(string name) => Name = name;
        }
    }
}
