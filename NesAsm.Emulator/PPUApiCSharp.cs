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

    public static void LoadImage(string filePath, bool hasTileSeparator = true)
    {
        var outputFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var path = Path.Combine(outputFolder!, filePath);

        if (File.Exists(path))
        {
            NesColorPalette[] backgroundPalettes = new NesColorPalette[4];
            NesColorPalette[] spritePalettes = new NesColorPalette[4];

            for (int i = 0; i < 4; i++)
            {
                backgroundPalettes[i] = new NesColorPalette(
                [
                    PPU.BackgroundPalettes[i, 0],
                    PPU.BackgroundPalettes[i, 1],
                    PPU.BackgroundPalettes[i, 2],
                    PPU.BackgroundPalettes[i, 3],
                ]);
            }

            for (int i = 0; i < 4; i++)
            {
                spritePalettes[i] = new NesColorPalette(
                [
                    PPU.SpritePalettes[i, 0],
                    PPU.SpritePalettes[i, 1],
                    PPU.SpritePalettes[i, 2],
                    PPU.SpritePalettes[i, 3],
                ]);
            }

            var charData = CharHelpers.LoadImage(path, hasTileSeparator, backgroundPalettes, spritePalettes);

            if (charData.HasValue)
            {
                // Set Background Tiles
                var tileIndex = 0;
                foreach (var tileData in charData.Value.BackgroundTiles)
                {
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                        {
                            byte colorIndex = tileData.Pixels[x + y * 8];
                            SetPatternTablePixel(0, (tileIndex % 16) * 8 + x, (tileIndex / 16) * 8 + y, colorIndex);
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
                            byte colorIndex = tileData.Pixels[x + y * 8];
                            SetPatternTablePixel(1, (tileIndex % 16) * 8 + x, (tileIndex / 16) * 8 + y, colorIndex);
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

    public static void SetScrollPosition(byte nametable, byte x, byte y) =>
        PPU.SetScrollPosition(nametable, x, y);

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
