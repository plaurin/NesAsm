﻿using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.PPUExamples;

public class VerticalScrolling
{
    static int WindowX = 0000;
    static byte ScrollX = 0;

    static int WindowY = 0240;
    static byte ScrollY = 0;

    static byte ScrollNametable = 0;

    public static void Reset(CancellationToken? cancellationToken = null)
    {
        SetGameHeader(isVerticalMirroring: false);

        // Background palette
        SetBackgroundPaletteColors(0, 0x_01, 0x_15, 0x_2A, 0X_23);
        SetBackgroundPaletteColors(1, 0x_11, 0x_12, 0x_13, 0x_14);
        SetBackgroundPaletteColors(2, 0x_21, 0x_22, 0x_23, 0x_24);
        SetBackgroundPaletteColors(3, 0x_31, 0x_32, 0x_33, 0x_34);

        // Background Tile 0
        SetPatternTablePixel(0, 0, 0, 1);
        SetPatternTablePixel(0, 1, 1, 2);
        SetPatternTablePixel(0, 2, 2, 3);

        // Background Tile 1
        SetPatternTablePixel(0, 10, 0, 3);
        SetPatternTablePixel(0, 11, 0, 2);
        SetPatternTablePixel(0, 12, 0, 1);

        // Background Tile 2
        SetPatternTablePixel(0, 20, 0, 2);
        SetPatternTablePixel(0, 21, 2, 2);
        SetPatternTablePixel(0, 22, 4, 2);

        // Background Tile 3
        SetPatternTablePixel(0, 26, 0, 3);
        SetPatternTablePixel(0, 27, 3, 2);
        SetPatternTablePixel(0, 28, 6, 3);

        // Nametable
        SetNametableTile(0, 1, 1, 1);
        SetNametableTile(0, 2, 2, 2);
        SetNametableTile(0, 3, 3, 3);

        // Attribute table
        SetAttributeTablePalette(0, 3, 0, 1);
        SetAttributeTablePalette(0, 4, 4, 2);
        SetAttributeTablePalette(0, 0, 4, 3);

        // SpritePalette
        SetSpritePaletteColors(0, 0x_00, 0x_06, 0x_16, 0X_26);
        SetSpritePaletteColors(1, 0x_00, 0x_0A, 0x_1A, 0x_2A);
        SetSpritePaletteColors(2, 0x_00, 0x_02, 0x_12, 0x_22);
        SetSpritePaletteColors(3, 0x_00, 0x_08, 0x_18, 0x_38);

        // Sprite Tile 1
        SetPatternTablePixel(1, 10, 4, 1);
        SetPatternTablePixel(1, 11, 4, 2);
        SetPatternTablePixel(1, 12, 4, 3);

        // Sprites
        SetSpriteData(0, 20, 20, 1, 0, false, false, false);
        SetSpriteData(1, 30, 30, 1, 1, false, false, false);
        SetSpriteData(2, 40, 40, 1, 2, true, false, false);
        SetSpriteData(3, 50, 50, 1, 3, false, false, false);

        // Main Loop
        while (cancellationToken == null || !cancellationToken.Value.IsCancellationRequested)
        {
            WindowX += 1;
            WindowY += 1;

            // Scroll to game position
            ScrollX = (byte)(WindowX % 256);
            ScrollY = (byte)(WindowY % 240);

            ScrollNametable = (byte)(((WindowY / 240) % 2) * 2);
            SetScrollPosition(ScrollNametable, ScrollX, ScrollY);

            WaitForVBlank();
        }
    }

    public static void Nmi() { }
}