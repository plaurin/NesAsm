namespace NesAsm.Emulator;

public class PPU
{
    private static byte[,] _backgroundPalette = new byte[4, 4];
    private static byte[,] _spritePalette = new byte[4, 4];
    private static byte[,,] _nametables = new byte[4, 32, 30];
    private static byte[,,] _attributeTables = new byte[4, 8, 8];
    private static byte[,,] _patternTables = new byte[2, 128, 128];

    public static void SetBackgroundPaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3)
    {
        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _backgroundPalette[paletteIndex, 0] = color0;
        _backgroundPalette[paletteIndex, 1] = color1;
        _backgroundPalette[paletteIndex, 2] = color2;
        _backgroundPalette[paletteIndex, 3] = color3;
    }

    public static void SetPatternTablePixel(int tableNumber, int x, int y, byte colorIndex)
    {
        if (tableNumber < 0 || tableNumber > 1)
            throw new ArgumentOutOfRangeException(nameof(tableNumber), "must be between 0 and 1");

        if (x < 0 || x > 127)
            throw new ArgumentOutOfRangeException(nameof(x), "must be between 0 and 127");

        if (y < 0 || y > 127)
            throw new ArgumentOutOfRangeException(nameof(y), "must be between 0 and 127");

        if (colorIndex < 0 || colorIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(colorIndex), "must be between 0 and 3");

        _patternTables[tableNumber, x, y] = colorIndex;
    }

    public static byte[] DrawScreen()
    {
        var screen = new byte[256 * 240];

        for (int y = 0; y < 240; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                screen[x + y * 256] = DrawPixel(x, y);
            }
        }

        return screen;
    }

    private static byte DrawPixel(int x, int y)
    {
        var (nametableX, nametableY) = GetNametablePosition(x, y);
        var patternIndex = _nametables[0, nametableX, nametableY];

        var (patternX, patternY) = GetPatternTablePosition(x, y);
        var colorIndex = _patternTables[0, (patternIndex % 16) * 8 + patternX, (patternIndex / 16) * 8 + patternY];

        if (colorIndex == 0)
        {
            return _backgroundPalette[0, 0];
        }

        var (attributeX, attributeY) = GetAttributeTablePosition(nametableX, nametableY);
        var paletteIndex = _attributeTables[0, attributeX, attributeY];

        return _backgroundPalette[paletteIndex, colorIndex];
    }

    internal static (int nametableX, int nametableY) GetNametablePosition(int screenX, int screenY) => (screenX / 8, screenY / 8);
    internal static (int patternX, int patternY) GetPatternTablePosition(int screenX, int screenY) => (screenX % 8, screenY % 8);
    internal static (int attributeX, int attributeY) GetAttributeTablePosition(int nametableX, int nametableY) => (nametableX / 4, nametableY / 4);
}