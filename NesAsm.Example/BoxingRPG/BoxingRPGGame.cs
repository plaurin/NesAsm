using NesAsm.Emulator;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    public static void Reset(CancellationToken? cancellationToken = null)
    {
        SetGameHeader(isVerticalMirroring: false);

        // Background palette
        SetBackgroundPaletteColors(GroundPaletteIndex, GroundPalette);
        SetBackgroundPaletteColors(SkyPaletteIndex, SkyPalette);
        SetBackgroundPaletteColors(BBoxerPaletteIndex, BBoxerPalette);
        SetBackgroundPaletteColors(HillPaletteIndex, HillPalette);

        SetSpritePaletteColors(BoxerPaletteIndex, BoxerPalette);
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
        //for (int i = 0; i < 32; i++)
        //{
        //    DrawBlock(i, 0, SkyTiles, HudPalette);
        //    DrawBlock(i, 1, SkyTiles, HudPalette);
        //}

        DrawWord(3, 2, "HERO");
        //DrawNumber(3, 3, 300, 6);

        //DrawBlock(5, 1, HudCoinTiles, HudCoinPalette);
        //DrawTile(12, 3, HudXTile);
        //DrawNumber(13, 3, 2, 2);

        DrawWord(18, 2, "HP");
        DrawNumber(19, 3, 99);
        //DrawDigit(19, 3, '1');
        //DrawCharacter(20, 3, '-');
        //DrawDigit(21, 3, '1');

        //DrawWord(25, 2, "TIME");
        //DrawNumber(26, 3, 297);

        // DrawBoxer
        DrawMetaTile(15, 24, BBoxerPaletteIndex, BoxerTiles);

        // Main Loop
        while (cancellationToken == null || !cancellationToken.Value.IsCancellationRequested)
        {
            //MainLoopBeforeSpriteZeroHit();
            //WaitForSpriteZeroHit();
            MainLoop();
            WaitForIrq(Irq); // After Header
            WaitForIrq(Irq); // After Sky
            WaitForIrq(Irq); // After Ground
            WaitForVBlank();
        }
    }

    static int WindowX = 0000;
    static int WindowY = 0050;
    const int DrawAheadColumns = 20;

    static byte Coins = 90;
    static int Time = 400;
    static byte BoxerX = 100;
    static byte BoxerY = 208;
    static byte SlimeX = 132;
    static byte SlimeY = 94 + 32;

    private static void MainLoop()
    {
        SetIRQAtScanline(32);

        // Scroll to draw static HUD
        SetScrollPosition(0, 0, 0);

        if (InputManager.Left) WindowX -= 1;
        if (InputManager.Right) WindowX += 1;
        if (InputManager.Up) WindowY -= 1;
        if (InputManager.Down) WindowY += 1;

        if (WindowY < 0) WindowY = 0;
        if (WindowY > 50) WindowY = 50;

        // Update HUD
        if (FrameCount % 15 == 0) Time--;
        if (Time < 0) Time = 0;

        if (FrameCount % 20 == 0) Coins++;
        if (Coins >= 100) Coins -= 100;

        // Coin
        //DrawNumber(13, 3, Coins, 2);

        // Time
        //DrawNumber(26, 3, Time);

        if (InputManager.Left) BoxerX -= 1;
        if (InputManager.Right) BoxerX += 1;
        if (InputManager.Up) BoxerY -= 1;
        if (InputManager.Down) BoxerY += 1;

        if (InputManager.B) SlimeX -= 1;
        if (InputManager.A) SlimeX += 1;
        if (InputManager.Select) SlimeY -= 1;
        if (InputManager.Start) SlimeY += 1;

        // Update Boxer
        //DrawMetaSpriteAnimation(1, BoxerX, BoxerY, BoxerPaletteIndex, BoxerIdle, FrameCount, 16);

        // Update Goomba
        SlimeY = (byte)(94 + 32 + 50 - WindowY);
        DrawMetaSpriteAnimation(30, SlimeX, SlimeY, SlimePaletteIndex, SlimeIdleFrames, FrameCount, 32);
    }

    static int FrameCount = 0;
    static byte ScrollX = 0;
    static byte ScrollY = 0;
    static byte ScrollNametable = 0;
    public static void Nmi()
    {
        FrameCount++;

        //var colorAnimFrame = FrameCount % 48;
        //if (colorAnimFrame < 8)
        //    SetBackgroundPaletteColors(3, 0x_0F, 0x_17, 0x_17, 0x_0F);
        //else if (colorAnimFrame < 16)
        //    SetBackgroundPaletteColors(3, 0x_0F, 0x_07, 0x_17, 0x_0F);
        //else if (colorAnimFrame < 24)
        //    SetBackgroundPaletteColors(3, 0x_0F, 0x_17, 0x_17, 0x_0F);
        //else
        //    SetBackgroundPaletteColors(3, 0x_0F, 0x_27, 0x_17, 0x_0F);
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
                SetIRQAtScanline((byte)(176 - ScrollY));
                break;
            case 1: // After Sky 
                SetScrollPosition(0, 0, (byte)(ScrollY + (50 - ScrollY) / 4));

                IrqCount++;
                SetIRQAtScanline(240 - 32);
                break;
            case 2: // After Ground
                SetScrollPosition(2, 128, 32);

                IrqCount = 0;
                break;
        }

    }
}