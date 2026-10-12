namespace NesAsm.Emulator;

public class NesRunner
{
    private PPUInstance _ppu;
    private NesMemory _memory;
    private CPU _cpu;

    private readonly byte[] _screen = new byte[256 * 240];
    public NesRunner(Cart cart)
    {
        _memory = new NesMemory(cart);
        _ppu = new PPUInstance(_memory);
        _memory.SetPPU(_ppu);
        _cpu = new CPU(_memory);
        _memory.SetCPU(_cpu);
    }

    public byte[] GetScreen() => _screen;

    public Task RunGame(CancellationToken cancellationToken, Action draw)
    {
        try
        {
            _cpu.Init();

            while (_ppu.Frame <= 1250)
            {
                if (_cpu.PC == 0x8094) { }

                var ins = _cpu.RunNextInstruction();

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

    public void DrawScreen(byte startScanline = 0, byte endScanline = 239)
    {
        bool setbackgroundcolor = true;
        bool drawSprites = (_ppu.PpuMask & 0x10) != 0;
        bool drawBackground = (_ppu.PpuMask & 0x08) != 0;

        if (setbackgroundcolor)
            for (int y = startScanline; y <= endScanline; y++)
                for (int x = 0; x < 256; x++)
                    _screen[x + y * 256] = _ppu.BackgroundColorIndex;

        byte GetSpriteY(int sprite) => _ppu.Oam[sprite * 4 + 0];
        byte GetSpriteX(int sprite) => _ppu.Oam[sprite * 4 + 3];
        bool IsSpriteBehindBackground(int sprite) => (_ppu.Oam[sprite * 4 + 2] & 0x20) != 0;

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

        if (drawBackground)
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

    private int BackgroundPatternTableIndex => ((_ppu.PpuCtrl & 0x10) != 0) ? 1 : 0;
    private int SpritePatternTableIndex => ((_ppu.PpuCtrl & 0x08) != 0) ? 1 : 0;
    private byte ScrollX => _ppu.ScrollX;
    private byte ScrollY => _ppu.ScrollY;
    private bool VerticalMirroring = true; // Default mirroring mode, can be changed by the game.

    private byte DrawBackgroundPixel(int x, int y, byte colorIndexIfTransparent)
    {
        (int nametableX, int nametableY) GetNametablePosition(int screenX, int screenY) => (screenX / 8, screenY / 8);
        (int patternX, int patternY) GetPatternTablePosition(int screenX, int screenY) => (screenX % 8, screenY % 8);
        (int attributeX, int attributeY) GetAttributeTablePosition(int nametableX, int nametableY) => (nametableX / 4, nametableY / 4);
        byte GetPatternIndex(int nametableIndex, int nametableX, int nametableY) => _ppu.GetVram((ushort)(0x2000 + nametableIndex * 0x400 + nametableY * 32 + nametableX));
        byte GetAttributeTable(int tableIndex, int x, int y) => _ppu.GetVram((ushort)(0x23C0 + tableIndex * 0x400 + y * 8 + x));
        byte GetBackgroundPaletteColor(int paletteIndex, int colorIndex) => _ppu.GetVram((ushort)(0x3F00 + paletteIndex * 4 + colorIndex));

        var nametableIndex = _ppu.PpuCtrl & 0x03;
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

        if (patternIndex != 0x24) { }

        var (patternX, patternY) = GetPatternTablePosition(x + ScrollX, y + ScrollY);
        var colorIndex = GetPatternTableColorIndex(BackgroundPatternTableIndex, (patternIndex % 16) * 8 + patternX, (patternIndex / 16) * 8 + patternY);

        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        var (attributeX, attributeY) = GetAttributeTablePosition(nametableX, nametableY);
        var attributeValue = GetAttributeTable(nametableIndex, attributeX, attributeY);
        if (attributeValue != 0) { }
        var bitIndex = (nametableX % 2) * 0x04 + (nametableY % 2) * 0x10 + 1;
        var bitIndex2 = (0x03 << ((nametableX % 2) * 2 + (nametableY % 2) * 4));
        var bitIndex3 = (nametableX % 2) * 2 + (nametableY % 2) * 4;
        var bit0 = attributeValue & bitIndex;
        var bit1 = attributeValue & (bitIndex << 1);
        var r = attributeValue >> bitIndex3 & 0x03;
        var paletteIndex = r;

        return GetBackgroundPaletteColor(paletteIndex, colorIndex);
    }

    private byte DrawSpritePixel(int spriteIndex, int x, int y, byte colorIndexIfTransparent)
    {
        // TODO Move up callstack
        byte GetSpriteTileIndex(int sprite) => _ppu.Oam[sprite * 4 + 1];
        byte GetSpritePaletteIndex(int sprite) => (byte)(_ppu.Oam[sprite * 4 + 2] & 0x03);
        bool IsSpriteFlippedHorizontally(int sprite) => (_ppu.Oam[sprite * 4 + 2] & 0x40) != 0;
        bool IsSpriteFlippedVertically(int sprite) => (_ppu.Oam[sprite * 4 + 2] & 0x80) != 0;
        byte GetSpritePaletteColor(int paletteIndex, int colorIndex) => 0;

        var patternIndex = GetSpriteTileIndex(spriteIndex);

        var actualX = IsSpriteFlippedHorizontally(spriteIndex) ? 7 - x : x;
        var actualY = IsSpriteFlippedVertically(spriteIndex) ? 7 - y : y;

        var colorIndex = GetPatternTableColorIndex(SpritePatternTableIndex, (patternIndex % 16) * 8 + actualX, (patternIndex / 16) * 8 + actualY);
        if (colorIndex == 0)
        {
            return colorIndexIfTransparent;
        }

        var paletteIndex = GetSpritePaletteIndex(spriteIndex);

        return GetSpritePaletteColor(paletteIndex, colorIndex);
    }

    private byte GetPatternTableColorIndex(int tableIndex, int x, int y)
    {
        var tx = (x / 8) * 0x10;
        var ty = (y / 8) * 0x100;
        var plane0 = _ppu.GetVram((ushort)(tableIndex * 0x1000 + tx + ty + y % 8));
        var plane1 = _ppu.GetVram((ushort)(tableIndex * 0x1000 + tx + ty + y % 8 + 8));
        var bit0 = plane0 >> (7 - x % 8) & 1;
        var bit1 = plane1 >> (7 - x % 8) & 1;
        return (byte)(bit0 | bit1 << 1);
    }
}
