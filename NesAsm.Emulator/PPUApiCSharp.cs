using NesAsm.Utilities;
using System.Reflection;

namespace NesAsm.Emulator;

public class PPUApiCSharp
{
    public static void SetBackgroundPaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3) =>
        PPU.SetBackgroundPaletteColors(paletteIndex, color0, color1, color2, color3);

    public static void SetSpritePaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3) =>
        PPU.SetSpritePaletteColors(paletteIndex, color0, color1, color2, color3);

    public static void SetSpriteData(byte spriteIndex, byte x, byte y, byte tileIndex, byte paletteIndex, bool isBehindBackground, bool flipHorizontally, bool flipVertically) =>
        PPU.SetSpriteData(spriteIndex, x, y, tileIndex, paletteIndex, isBehindBackground, flipHorizontally, flipVertically);

    public static void SetPatternTablePixel(int tableNumber, int x, int y, byte colorIndex) =>
        PPU.SetPatternTablePixel(tableNumber, x, y, colorIndex);

    public static void SetNametableTile(int tableNumber, int x, int y, byte tileIndex) =>
        PPU.SetNametableTile(tableNumber, x, y, tileIndex);

    public static void SetAttributeTablePalette(int tableNumber, int x, int y, byte paletteIndex) =>
        PPU.SetAttributeTablePalette(tableNumber, x, y, paletteIndex);

    public static void LoadImage(string filePath)
    {
        var outputFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var path = Path.Combine(outputFolder!, filePath);

        if (File.Exists(path))
        {
            var charData = CharHelpers.LoadImage(path);

            if (charData.HasValue)
            {
                // Set Palettes
                int paletteIndex = 0;
                foreach (var palette in charData.Value.BackgroundPalettes.Take(4))
                {
                    SetBackgroundPaletteColors(paletteIndex++, palette.NesColors[0], palette.NesColors[1], palette.NesColors[2], palette.NesColors[3]);
                }

                paletteIndex = 0;
                foreach (var palette in charData.Value.SpritePalettes.Take(4))
                {
                    SetSpritePaletteColors(paletteIndex++, palette.NesColors[0], palette.NesColors[1], palette.NesColors[2], palette.NesColors[3]);
                }

                // Set Background Tiles
                var tileIndex = 0;
                foreach (var tileData in charData.Value.BackgroundTiles)
                {
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                        {
                            byte colorIndex = tileData.Palette.GetColorIndex(tileData.Pixels[x + y * 8]);
                            SetPatternTablePixel(0, (tileIndex % 8) * 8 + x, (tileIndex / 8) * 8 + y, colorIndex);
                        }
                    tileIndex++;
                }

                // Set Sprite Tiles
                tileIndex = 0;
                foreach (var tileData in charData.Value.SpriteTiles)
                {
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                        {
                            byte colorIndex = tileData.Palette.GetColorIndex(tileData.Pixels[x + y * 8]);
                            SetPatternTablePixel(1, (tileIndex % 8) * 8 + x, (tileIndex / 8) * 8 + y, colorIndex);
                        }
                    tileIndex++;
                }
            }
        }
        else
        {
            throw new FileNotFoundException("Don't forget to copy image file to output!", filePath);
        }
    }

    public static void WaitForVBlank()
    {
        _nmiCallback?.Invoke();
    }

    private static Action? _nmiCallback = null;

    internal static void SetNmiCallback(Action callback)
    {
        _nmiCallback = callback;
    }
}
