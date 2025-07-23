using NesAsm.Emulator;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    const byte SplitSkyGround = 94;
    const byte SkyScrollMax = 50;
    const byte HeaderHeight = 32;
    const byte FooterHeight = 32;
    const byte SplitSkyGroundMemory = SplitSkyGround + HeaderHeight + SkyScrollMax;

    public static void Reset(CancellationToken? cancellationToken = null)
    {
        SetGameHeader(isVerticalMirroring: false);

        // Background palette
        SetBackgroundPaletteColors(GroundPaletteIndex, GroundPalette);
        SetBackgroundPaletteColors(SkyPaletteIndex, SkyPalette);
        SetBackgroundPaletteColors(BoxerPaletteIndex, BoxerPalette);
        SetBackgroundPaletteColors(HillPaletteIndex, HillPalette);

        SetSpritePaletteColors(ShadowPaletteIndex, ShadowPalette);
        SetSpritePaletteColors(SlimePaletteIndex, SlimePalette);

        // Init;
        byte[] importColors = [0x_22, 0x_2D, 0x_10, 0x_20];
        LoadImage(@"BoxingRPG\BoxingRPG.png", importColors, hasTileSeparator: false);

        LoadFightWith("Slime");

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        // Draw Field
        DrawBlockFill(0, 0, 16, 11, SkyTiles, SkyPaletteIndex);
        DrawBlockFill(0, 11, 16, 15, GroundTiles, GroundPaletteIndex);

        DrawBlock(1, 10, BushLeftTiles, BushPaletteIndex);
        DrawBlock(2, 10, BushTiles, BushPaletteIndex);
        DrawBlock(3, 10, BushRightTiles, BushPaletteIndex);

        DrawBlock(11, 10, HillLeftTiles, HillPaletteIndex);
        DrawBlock(12, 10, HillSpotTiles, HillPaletteIndex);
        DrawBlock(12, 9, HillTopTiles, HillPaletteIndex);
        DrawBlock(13, 10, HillRightTiles, HillPaletteIndex);

        DrawBlock(5, 6, CloudTopLeftTiles, CloudPaletteIndex);
        DrawBlock(5, 7, CloudBottomLeftTiles, CloudPaletteIndex);
        DrawBlock(6, 6, CloudTopTiles, CloudPaletteIndex);
        DrawBlock(6, 7, CloudBottomTiles, CloudPaletteIndex);
        DrawBlock(7, 6, CloudTopRightTiles, CloudPaletteIndex);
        DrawBlock(7, 7, CloudBottomRightTiles, CloudPaletteIndex);

        DrawWord(0, 21, "PING");
        DrawWord(0, 22, "PONG");
        SetPalette(0, 11, HillPaletteIndex);

        // Draw HUD
        DrawWord(3, 2, "HERO");

        DrawWord(18, 2, "HP");
        DrawNumber(19, 3, 99);

        // DrawBoxer
        DrawMetaTile(15, 24, BoxerPaletteIndex, BoxerTiles);

        // Main Loop
        while (cancellationToken == null || !cancellationToken.Value.IsCancellationRequested)
        {
            MainLoop();
            WaitForIrq(Irq); // After Header
            WaitForIrq(Irq); // After Sky
            WaitForIrq(Irq); // After Ground
            WaitForVBlank();
        }
    }

    static int WindowX = 0000;
    static int WindowY = SkyScrollMax;

    static byte BoxerX = 100;
    static byte BoxerY = 208;
    static byte SlimeX = 132;
    static byte SlimeY = SplitSkyGround + HeaderHeight;

    private static void MainLoop()
    {
        SetIRQAtScanline(HeaderHeight);

        // Scroll to draw static HUD
        SetScrollPosition(0, 0, 0);

        if (InputManager.Left) WindowX -= 1;
        if (InputManager.Right) WindowX += 1;
        if (InputManager.Up) WindowY -= 1;
        if (InputManager.Down) WindowY += 1;

        if (WindowY < 0) WindowY = 0;
        if (WindowY > SkyScrollMax) WindowY = SkyScrollMax;

        // Update HUD

        // Input Update

        if (InputManager.Left) BoxerX -= 1;
        if (InputManager.Right) BoxerX += 1;
        if (InputManager.Up) BoxerY -= 1;
        if (InputManager.Down) BoxerY += 1;

        if (InputManager.B) SlimeX -= 1;
        if (InputManager.A) SlimeX += 1;
        if (InputManager.Select) SlimeY -= 1;
        if (InputManager.Start) SlimeY += 1;

        // Update Sprites
        
        SpriteIndex = 0;

        // Update Slime
        SlimeY = (byte)(8 + SplitSkyGround + HeaderHeight + SkyScrollMax - WindowY);
        DrawMetaSprite(ref SpriteIndex, (byte)(SlimeX + 3), (byte)(SlimeY + 3), ShadowPaletteIndex, SlimeShadow, drawBehindBackground: false);

        DrawMetaSpriteAnimation(ref SpriteIndex, SlimeX, SlimeY, SlimePaletteIndex, SlimeIdleFrames, FrameCount, 32, drawBehindBackground: false);
    }

    static byte SpriteIndex = 0;
    static int FrameCount = 0;
    static byte ScrollX = 0;
    static byte ScrollY = 0;
    static byte ScrollNametable = 0;
    public static void Nmi()
    {
        FrameCount++;
    }

    static byte IrqCount = 0;
    public static void Irq()
    {
        switch (IrqCount)
        {
            case 0: // After Header
                // Scroll to game position
                ScrollX = (byte)(WindowX % 256);
                ScrollY = (byte)(WindowY % 240);

                ScrollNametable = (byte)(((WindowY / 240) % 2) * 2);
                SetScrollPosition(ScrollNametable, ScrollX, ScrollY);

                IrqCount++;
                SetIRQAtScanline((byte)(SplitSkyGroundMemory - ScrollY));
                break;
            case 1: // After Sky 
                SetScrollPosition(0, 0, (byte)(ScrollY + (SkyScrollMax - ScrollY) / 4));

                IrqCount++;
                SetIRQAtScanline(240 - FooterHeight);
                break;
            case 2: // After Ground
                SetScrollPosition(2, 123, FooterHeight);

                IrqCount = 0;
                break;
        }

    }
}