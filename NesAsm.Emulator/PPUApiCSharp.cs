﻿using NesAsm.Utilities;
using System.Reflection;

namespace NesAsm.Emulator;

public class PPUApiCSharp
{
    public static void SetGameHeader(bool isVerticalMirroring = true, string mapperName = "NROM")
    {
        if (isVerticalMirroring)
            PPU.SetVerticalMirroring();
        else
            PPU.SetHorizontalMirroring();
    }

    public static void SetBackgroundPaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3) =>
        PPU.SetBackgroundPaletteColors(paletteIndex, color0, color1, color2, color3);

    public static void SetSpritePaletteColors(int paletteIndex, byte color0, byte color1, byte color2, byte color3) =>
        PPU.SetSpritePaletteColors(paletteIndex, color0, color1, color2, color3);

    public static void SetSpriteData(byte spriteIndex, byte x, byte y, byte tileIndex, byte paletteIndex, bool isBehindBackground = false, bool flipHorizontally = false, bool flipVertically = false) =>
        PPU.SetSpriteData(spriteIndex, x, y, tileIndex, paletteIndex, isBehindBackground, flipHorizontally, flipVertically);

    public static void SetPatternTablePixel(int tableNumber, int x, int y, byte colorIndex) =>
        PPU.SetPatternTablePixel(tableNumber, x, y, colorIndex);

    public static void SetNametableTile(int tableNumber, int x, int y, byte tileIndex) =>
        PPU.SetNametableTile(tableNumber, x, y, tileIndex);

    public static void SetAttributeTablePalette(int tableNumber, int x, int y, byte paletteIndex) =>
        PPU.SetAttributeTablePalette(tableNumber, x, y, paletteIndex);

    public static void LoadImage(string filePath, byte[] importColors, bool hasTileSeparator = true)
    {
        if (importColors.Length != 4)
            throw new ArgumentException("Import colors must have exactly 4 colors.");

        NesColorPalette[] backgroundPalettes = new NesColorPalette[4];
        NesColorPalette[] spritePalettes = new NesColorPalette[4];

        for (int i = 0; i < 4; i++)
        {
            backgroundPalettes[i] = new NesColorPalette([importColors[0], importColors[1], importColors[2], importColors[3]]);
            spritePalettes[i] = new NesColorPalette([importColors[0], importColors[1], importColors[2], importColors[3]]);
        }


        LoadImage(filePath, backgroundPalettes, spritePalettes, hasTileSeparator);
    }

    public static void LoadImage(string filePath, bool hasTileSeparator = true)
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

            spritePalettes[i] = new NesColorPalette(
            [
                PPU.SpritePalettes[i, 0],
                PPU.SpritePalettes[i, 1],
                PPU.SpritePalettes[i, 2],
                PPU.SpritePalettes[i, 3],
            ]);
        }

        LoadImage(filePath, backgroundPalettes, spritePalettes, hasTileSeparator);
    }

    private static void LoadImage(string filePath, NesColorPalette[] backgroundPalettes, NesColorPalette[] spritePalettes, bool hasTileSeparator)
    {
        var outputFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        var path = Path.Combine(outputFolder!, filePath);

        if (File.Exists(path))
        {
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
        if (!_spriteZeroHitScanline.HasValue && !_irqAtScanline.HasValue)
            PPU.DrawScreen();
        else if (_spriteZeroHitScanline.HasValue)
            PPU.DrawScreen(startScanline: (byte)(_spriteZeroHitScanline.Value + 1), endScanline: 239);
        else if (_irqAtScanline.HasValue)
        {
            PPU.DrawScreen(startScanline: _lastIrqAtScanline, endScanline: 239);
            _lastIrqAtScanline = 0;
        }

        _nmiCallback?.Invoke();
    }

    private static Action? _nmiCallback = null;

    internal static void SetNmiCallback(Action callback)
    {
        _nmiCallback = callback;
    }

    public static void WaitForSpriteZeroHit()
    {
        // TODO Validate than Sprite0 indeed is possitioned to hit at _spriteZeroHitScanline

        if (!_spriteZeroHitScanline.HasValue)
            throw new InvalidOperationException("Need to SetSpriteZeroHitScanline");

        PPU.DrawScreen(startScanline: 0, endScanline: _spriteZeroHitScanline.Value);
    }

    public static void WaitForIrq(Action interrupt)
    {
        if (!_irqAtScanline.HasValue)
            throw new InvalidOperationException("Need to SetIRQAtScanline");

        PPU.DrawScreen(startScanline: _lastIrqAtScanline, endScanline: _irqAtScanline.Value);
        _lastIrqAtScanline = _irqAtScanline.Value;

        interrupt.Invoke();
    }

    private static byte? _spriteZeroHitScanline = null;

    public static void SetSpriteZeroHitScanline(byte scanline)
    {
        _spriteZeroHitScanline = scanline;
    }

    private static byte? _irqAtScanline = null;
    private static byte _lastIrqAtScanline = 0;

    public static void SetIRQAtScanline(byte scanline)
    {
        _irqAtScanline = scanline;
    }
}
