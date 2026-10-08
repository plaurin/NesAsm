namespace NesAsm.Emulator;

public class NesRunner
{
    private PPUInstance _ppu;
    private NesMemory _memory;
    private CPU _cpu;

    private readonly byte[] _screen = new byte[256 * 240];
    public NesRunner(Cart cart)
    {
        _ppu = new PPUInstance();
        _memory = new NesMemory(cart, _ppu);
        _cpu = new CPU(_memory);
        _memory.SetCPU(_cpu);
    }

    public byte[] GetScreen() => _screen;

    public Task RunGame(CancellationToken cancellationToken, Action draw)
    {
        try
        {
            _cpu.Init();

            while (_ppu.Frame <= 250)
            {
                if (_cpu.PC == 0x9662) { }

                _cpu.RunNextInstruction();

                _ppu.RunToCycle(_cpu.Cycles);

                if (_ppu.NmiRequested)
                {
                    _ppu.NmiRequested = false;

                    DrawScreen();
                    draw.Invoke();

                    InputManager.FrameUpdate();
                    _cpu.RunNmi();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return Task.CompletedTask;
    }

    private const byte SpriteYOffset = 1; // The PPU adds 1 to the sprite Y position, so we need to subtract it when setting the sprite Y position.

    public void DrawScreen(bool drawSprites = true, bool setbackgroundcolor = true, byte startScanline = 0, byte endScanline = 239)
    {
        if (setbackgroundcolor)
            for (int y = startScanline; y <= endScanline; y++)
                for (int x = 0; x < 256; x++)
                    _screen[x + y * 256] = _ppu.BackgroundColorIndex;

        byte GetSpriteY(int sprite) => 0;
        byte GetSpriteX(int sprite) => 0;
        bool IsSpriteBehindBackground(int sprite) => false;

        void drawSprite(int index)
        {
            for (int y = 0; y < 8; y++)
                if (GetSpriteY(index) + y + SpriteYOffset >= startScanline && GetSpriteY(index) + y + SpriteYOffset <= endScanline)
                    for (int x = 0; x < 8; x++)
                    {
                        var screenIndex = (GetSpriteX(index) + x) + (GetSpriteY(index) + y + SpriteYOffset) * 256;
                        if (screenIndex < _screen.Length)
                            _screen[screenIndex] = DrawSpritePixel(index, x, y, _screen[screenIndex]);
                    }
        }

        if (drawSprites)
            for (int i = 0; i < 64; i++)
                if (IsSpriteBehindBackground(i))
                    drawSprite(i);

        for (int y = startScanline; y <= endScanline; y++)
            for (int x = 0; x < 256; x++)
            {
                var screenIndex = x + y * 256;
                _screen[screenIndex] = DrawBackgroundPixel(x, y, _screen[screenIndex]);
            }

        if (drawSprites)
            for (int i = 0; i < 64; i++)
                if (!IsSpriteBehindBackground(i) && GetSpriteY(i) < 240)
                    drawSprite(i);

    }

    private byte BackgroundPatternTableIndex = 0;
    private byte SpritePatternTableIndex = 1;
    private byte ScrollNametable => 0;
    private byte ScrollX => 0;
    private byte ScrollY => 0;
    private bool VerticalMirroring = true; // Default mirroring mode, can be changed by the game.

    private byte DrawBackgroundPixel(int x, int y, byte colorIndexIfTransparent)
    {
        (int nametableX, int nametableY) GetNametablePosition(int screenX, int screenY) => (screenX / 8, screenY / 8);
        (int patternX, int patternY) GetPatternTablePosition(int screenX, int screenY) => (screenX % 8, screenY % 8);
        (int attributeX, int attributeY) GetAttributeTablePosition(int nametableX, int nametableY) => (nametableX / 2, nametableY / 2);
        byte GetPatternIndex(int nametableIndex, int nametableX, int nametableY) => 0;

        var nametableIndex = ScrollNametable;
        var (nametableX, nametableY) = GetNametablePosition(x + ScrollX, y + ScrollY);
        if (nametableX > 31)
        {
            if (VerticalMirroring) nametableIndex = (byte)((nametableIndex + 1) % 2);
            nametableX -= 32;
        }

        if (nametableY > 29)
        {
            if (!VerticalMirroring) nametableIndex = (byte)((nametableIndex + 2) % 4);
            nametableY -= 30;
        }

        var patternIndex = GetPatternIndex(nametableIndex, nametableX, nametableY);

        var (patternX, patternY) = GetPatternTablePosition(x + ScrollX, y + ScrollY);
        var colorIndex = GetPatternTable(BackgroundPatternTableIndex, (patternIndex % 16) * 8 + patternX, (patternIndex / 16) * 8 + patternY);

        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        var (attributeX, attributeY) = GetAttributeTablePosition(nametableX, nametableY);
        var paletteIndex = GetAttributeTable(nametableIndex, attributeX, attributeY);

        return GetBackgroundPaletteColor(paletteIndex, colorIndex);
    }

    private byte GetAttributeTable(int tableIndex, int x, int y)
    {
        return 0;
    }

    private byte GetBackgroundPaletteColor(int paletteIndex, int colorIndex) => 0;

    private byte DrawSpritePixel(int spriteIndex, int x, int y, byte colorIndexIfTransparent)
    {
        // TODO Move up callstack
        byte GetSpriteTileIndex(int sprite) => 0;
        byte GetSpritePaletteIndex(int sprite) => 0;
        bool IsSpriteFlippedHorizontally(int sprite) => false;
        bool IsSpriteFlippedVertically(int sprite) => false;

        var patternIndex = GetSpriteTileIndex(spriteIndex);

        var actualX = IsSpriteFlippedHorizontally(spriteIndex) ? 7 - x : x;
        var actualY = IsSpriteFlippedVertically(spriteIndex) ? 7 - y : y;

        var colorIndex = GetPatternTable(1, (patternIndex % 16) * 8 + actualX, (patternIndex / 16) * 8 + actualY);
        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        var paletteIndex = GetSpritePaletteIndex(spriteIndex);

        return GetSpritePaletteColor(paletteIndex, colorIndex);
    }

    private byte GetPatternTable(int tableIndex, int x, int y)
    {
        return 0;
    }

    private byte GetSpritePaletteColor(int paletteIndex, int colorIndex) => 0;
}
