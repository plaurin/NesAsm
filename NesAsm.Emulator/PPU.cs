namespace NesAsm.Emulator;

public class PPU
{
    private const byte BackgroundPatternTableIndex = 0;
    private const byte SpritePatternTableIndex = 1;

    private static readonly byte[,] _backgroundPalette = new byte[4, 4];
    private static readonly byte[,] _spritePalette = new byte[4, 4];
    private static readonly byte[,,] _nametables = new byte[4, 32, 30];
    private static readonly byte[,,] _attributeTables = new byte[4, 16, 15];
    private static readonly byte[,,] _patternTables = new byte[2, 128, 128];

    private static readonly SpriteData[] _sprites = new SpriteData[64];
    private struct SpriteData
    {
        public byte x;
        public byte y;
        public byte tileIndex;
        public byte paletteIndex;
        public bool isBehindBackground;
        public bool flipHorizontally;
        public bool flipVertically;
    }

    private static byte ScrollNametable;
    private static byte ScrollX;
    private static byte ScrollY;

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

        new(255, 254, 255),
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

    public static byte[,] BackgroundPalettes => _backgroundPalette;

    public static byte[,] SpritePalettes => _spritePalette;

    public static void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                _backgroundPalette[i, j] = 0;
                _spritePalette[i, j] = 0;
            }

            for (int j = 0; j < 32; j++)
                for (int k = 0; k < 30; k++)
                    _nametables[i, j, k] = 0;

            for (int j = 0; j < 16; j++)
                for (int k = 0; k < 15; k++)
                    _attributeTables[i, j, k] = 0;
        }

        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 128; j++)
                for (int k = 0; k < 128; k++)
                    _patternTables[i, j, k] = 0;
    }

    private static readonly byte[] screen = new byte[256 * 240];
    public static byte[] GetScreen() => screen;
    public static byte[] DrawScreen(bool drawSprites = true, bool setbackgroundcolor = true, byte startScanline = 0, byte endScanline = 239)
    {
        if (setbackgroundcolor)
            for (int y = startScanline; y <= endScanline; y++)
                for (int x = 0; x < 256; x++)
                    screen[x + y * 256] = _backgroundPalette[0, 0];

        if (drawSprites)
        for (int i = 0; i < 64; i++)
            if (_sprites[i].isBehindBackground)
                for (int y = 0; y < 8; y++)
                    if (_sprites[i].y + y >= startScanline && _sprites[i].y + y <= endScanline)
                    for (int x = 0; x < 8; x++)
                    {
                        var screenIndex = (_sprites[i].x + x) + (_sprites[i].y + y) * 256;
                        screen[screenIndex] = DrawSpritePixel(i, x, y, screen[screenIndex]);
                    }

        for (int y = startScanline; y <= endScanline; y++)
            for (int x = 0; x < 256; x++)
            {
                var screenIndex = x + y * 256;
                screen[screenIndex] = DrawBackgroundPixel(x, y, screen[screenIndex]);
            }

        if (drawSprites)
        for (int i = 0; i < 64; i++)
            if (!_sprites[i].isBehindBackground && _sprites[i].y < 240)
                for (int y = 0; y < 8; y++)
                    if (_sprites[i].y + y >= startScanline && _sprites[i].y + y <= endScanline)
                    for (int x = 0; x < 8; x++)
                    {
                        var screenIndex = (_sprites[i].x + x) + (_sprites[i].y + y) * 256;
                        screen[screenIndex] = DrawSpritePixel(i, x, y, screen[screenIndex]);
                    }

        return screen;
    }

    private static byte DrawBackgroundPixel(int x, int y, byte colorIndexIfTransparent)
    {
        var nametableIndex = ScrollNametable;
        var (nametableX, nametableY) = GetNametablePosition(x + ScrollX, y + ScrollY);
        if (nametableX > 31) { nametableIndex = (byte)((nametableIndex + 1) % 2); nametableX -= 32; }

        var patternIndex = _nametables[nametableIndex, nametableX, nametableY];

        var (patternX, patternY) = GetPatternTablePosition(x + ScrollX, y + ScrollY);
        var colorIndex = _patternTables[BackgroundPatternTableIndex, (patternIndex % 16) * 8 + patternX, (patternIndex / 16) * 8 + patternY];

        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        var (attributeX, attributeY) = GetAttributeTablePosition(nametableX, nametableY);
        var paletteIndex = _attributeTables[nametableIndex, attributeX, attributeY];

        return _backgroundPalette[paletteIndex, colorIndex];
    }

    private static byte DrawSpritePixel(int spriteIndex, int x, int y, byte colorIndexIfTransparent)
    {
        var spriteData = _sprites[spriteIndex];

        var patternIndex = spriteData.tileIndex;
        var colorIndex = _patternTables[1, (patternIndex % 16) * 8 + x, (patternIndex / 16) * 8 + y];
        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        var paletteIndex = spriteData.paletteIndex;

        return _spritePalette[paletteIndex, colorIndex];
    }

    private static (int nametableX, int nametableY) GetNametablePosition(int screenX, int screenY) => (screenX / 8, screenY / 8);
    private static (int patternX, int patternY) GetPatternTablePosition(int screenX, int screenY) => (screenX % 8, screenY % 8);
    private static (int attributeX, int attributeY) GetAttributeTablePosition(int nametableX, int nametableY) => (nametableX / 2, nametableY / 2);

    internal static void SetBackgroundPaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3)
    {
        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _backgroundPalette[paletteIndex, 0] = color0;
        _backgroundPalette[paletteIndex, 1] = color1;
        _backgroundPalette[paletteIndex, 2] = color2;
        _backgroundPalette[paletteIndex, 3] = color3;
    }

    internal static void SetSpritePaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3)
    {
        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _spritePalette[paletteIndex, 0] = color0;
        _spritePalette[paletteIndex, 1] = color1;
        _spritePalette[paletteIndex, 2] = color2;
        _spritePalette[paletteIndex, 3] = color3;
    }

    internal static void SetSpriteData(byte spriteIndex, byte x, byte y, byte tileIndex, byte paletteIndex, bool isBehindBackground, bool flipHorizontally, bool flipVertically)
    {
        if (spriteIndex < 0 || spriteIndex > 63)
            throw new ArgumentOutOfRangeException(nameof(spriteIndex), "must be between 0 and 63");

        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(spriteIndex), "must be between 0 and 3");

        _sprites[spriteIndex] = new SpriteData
        {
            x = x,
            y = y,
            tileIndex = tileIndex,
            paletteIndex = paletteIndex,
            isBehindBackground = isBehindBackground,
            flipHorizontally = flipHorizontally,
            flipVertically = flipVertically
        };
    }

    internal static void SetPatternTablePixel(int tableNumber, int x, int y, byte colorIndex)
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

    internal static void SetNametableTile(int tableNumber, int x, int y, byte tileIndex)
    {
        if (tableNumber < 0 || tableNumber > 3)
            throw new ArgumentOutOfRangeException(nameof(tableNumber), "must be between 0 and 3");

        if (x < 0 || x > 31)
            throw new ArgumentOutOfRangeException(nameof(x), "must be between 0 and 31");

        if (y < 0 || y > 29)
            throw new ArgumentOutOfRangeException(nameof(y), "must be between 0 and 29");

        _nametables[tableNumber, x, y] = tileIndex;
    }

    internal static void SetAttributeTablePalette(int tableNumber, int x, int y, byte paletteIndex)
    {
        if (tableNumber < 0 || tableNumber > 3)
            throw new ArgumentOutOfRangeException(nameof(tableNumber), "must be between 0 and 3");

        if (x < 0 || x > 15)
            throw new ArgumentOutOfRangeException(nameof(x), "must be between 0 and 15");

        if (y < 0 || y > 14)
            throw new ArgumentOutOfRangeException(nameof(y), "must be between 0 and 29");

        if (paletteIndex < 0 || paletteIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(paletteIndex), "must be between 0 and 3");

        _attributeTables[tableNumber, x, y] = paletteIndex;
    }

    internal static void SetScrollPosition(byte nametable, byte x, byte y)
    {
        if (nametable < 0 || nametable > 3)
            throw new ArgumentOutOfRangeException(nameof(nametable), "must be between 0 and 3");

        if (y > 239)
            throw new ArgumentOutOfRangeException(nameof(y), "must be between 0 and 239");

        ScrollNametable = nametable;
        ScrollX = x;
        ScrollY = y;
    }

    internal static void SetScrollNametable(byte nametable)
    {
        ScrollNametable = nametable;
    }
}