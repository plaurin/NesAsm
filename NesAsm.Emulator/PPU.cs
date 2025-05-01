namespace NesAsm.Emulator;

public class PPU
{
    private static byte[,] _backgroundPalette = new byte[4, 4];
    private static byte[,] _spritePalette = new byte[4, 4];
    private static byte[,,] _nametables = new byte[4, 32, 30];
    private static byte[,,] _attributeTables = new byte[4, 16, 16];
    private static byte[,,] _patternTables = new byte[2, 128, 128];

    public static readonly (byte r, byte g, byte b)[] Colors =
    [
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
    ];

    public static void SetBackgroundPaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3)
    {
        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _backgroundPalette[paletteIndex, 0] = color0;
        _backgroundPalette[paletteIndex, 1] = color1;
        _backgroundPalette[paletteIndex, 2] = color2;
        _backgroundPalette[paletteIndex, 3] = color3;
    }

    public static void SetSpritePaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3)
    {
        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _spritePalette[paletteIndex, 0] = color0;
        _spritePalette[paletteIndex, 1] = color1;
        _spritePalette[paletteIndex, 2] = color2;
        _spritePalette[paletteIndex, 3] = color3;
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

    public static void SetNametableTile(int tableNumber, int x, int y, byte tileIndex)
    {
        if (tableNumber < 0 || tableNumber > 3)
            throw new ArgumentOutOfRangeException(nameof(tableNumber), "must be between 0 and 3");

        if (x < 0 || x > 31)
            throw new ArgumentOutOfRangeException(nameof(x), "must be between 0 and 31");

        if (y < 0 || y > 29)
            throw new ArgumentOutOfRangeException(nameof(y), "must be between 0 and 29");

        _nametables[tableNumber, x, y] = tileIndex;
    }

    public static void SetAttributeTablePalette(int tableNumber, int x, int y, byte paletteIndex)
    {
        if (tableNumber < 0 || tableNumber > 3)
            throw new ArgumentOutOfRangeException(nameof(tableNumber), "must be between 0 and 3");

        if (x < 0 || x > 15)
            throw new ArgumentOutOfRangeException(nameof(x), "must be between 0 and 31");

        if (y < 0 || y > 15)
            throw new ArgumentOutOfRangeException(nameof(y), "must be between 0 and 29");

        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _attributeTables[tableNumber, x, y] = paletteIndex;
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
    internal static (int attributeX, int attributeY) GetAttributeTablePosition(int nametableX, int nametableY) => (nametableX / 2, nametableY / 2);
}