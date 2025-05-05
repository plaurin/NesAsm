
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
