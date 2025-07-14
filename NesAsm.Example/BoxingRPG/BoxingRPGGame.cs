using NesAsm.Emulator;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    public static void Reset(CancellationToken? cancellationToken = null)
    {
        SetGameHeader(isVerticalMirroring: false);

        SetSpriteZeroHitScanline(31);

        // Background palette
        SetBackgroundPaletteColors(0, 0x_12, 0x_29, 0x_1A, 0X_0F); // SkyPalette
        SetBackgroundPaletteColors(1, 0x_0F, 0x_29, 0x_29, 0x_29); // GroundPalette
        SetBackgroundPaletteColors(2, 0x_0F, 0x_30, 0x_21, 0x_0F);
        SetBackgroundPaletteColors(3, 0x_0F, 0x_27, 0x_17, 0x_0F);

        SetSpritePaletteColors(BoxerPaletteIndex, BoxerPalette);
        SetSpritePaletteColors(SlimePaletteIndex, SlimePalette);

        // Init;
        byte[] importColors = [0x_22, 0x_16, 0x_27, 0x_18];
        LoadImage(@"BoxingRPG\BoxingRPG.png", importColors, hasTileSeparator: false);

        LoadFightWith("Slime");

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        // Draw Field
        DrawBlockFill(0, 0, 16, 11, SkyTiles, SkyPalette);
        DrawBlockFill(0, 11, 16, 15, GroundTiles, GroundPalette);

        DrawBlock(1, 10, BushLeftTiles, BushPalette);
        DrawBlock(2, 10, BushTiles, BushPalette);
        DrawBlock(3, 10, BushRightTiles, BushPalette);

        DrawBlock(11, 10, HillLeftTiles, HillPalette);
        DrawBlock(12, 10, HillSpotTiles, HillPalette);
        DrawBlock(12, 9, HillTopTiles, HillPalette);
        DrawBlock(13, 10, HillRightTiles, HillPalette);

        DrawBlock(5, 6, CloudTopLeftTiles, CloudPalette);
        DrawBlock(5, 7, CloudBottomLeftTiles, CloudPalette);
        DrawBlock(6, 6, CloudTopTiles, CloudPalette);
        DrawBlock(6, 7, CloudBottomTiles, CloudPalette);
        DrawBlock(7, 6, CloudTopRightTiles, CloudPalette);
        DrawBlock(7, 7, CloudBottomRightTiles, CloudPalette);

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

        // Main Loop
        while (cancellationToken == null || !cancellationToken.Value.IsCancellationRequested)
        {
            MainLoopBeforeSpriteZeroHit();
            WaitForSpriteZeroHit();
            MainLoopAfterSpriteZeroHit();
            WaitForVBlank();
        }
    }

    static int WindowX = 0000;
    static int WindowY = 0050;
    const int DrawAheadColumns = 20;

    private static void MainLoopBeforeSpriteZeroHit()
    {
        // Scroll to draw static HUD
        SetScrollPosition(0, 0, 0);

        // Do light weight stuff that we are sure will complete before the hud in the top finish to draw
        //DrawColumn((WindowX / 16) + DrawAheadColumns);
    }

    static byte Coins = 90;
    static int Time = 400;
    static byte BoxerX = 100;
    static byte BoxerY = 208;
    static byte SlimeX = 150;
    static byte SlimeY = 208;

    private static void MainLoopAfterSpriteZeroHit()
    {
        if (InputManager.Left) WindowX -= 1;
        if (InputManager.Right) WindowX += 1;
        if (InputManager.Up) WindowY -= 1;
        if (InputManager.Down) WindowY += 1;

        if (WindowY < 0) WindowY = 0;
        if (WindowY > 50) WindowY = 50;

        // Scroll to game position
        ScrollX = (byte)(WindowX % 256);
        ScrollY = (byte)(WindowY % 240);

        ScrollNametable = (byte)(((WindowY / 240) % 2) * 2);
        SetScrollPosition(ScrollNametable, ScrollX, ScrollY);

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
        DrawMetaSpriteAnimation(1, BoxerX, BoxerY, BoxerPaletteIndex, BoxerIdle, FrameCount, 16);

        // Update Goomba
        DrawMetaSpriteAnimation(30, SlimeX, SlimeY, SlimePaletteIndex, SlimeIdleFrames, FrameCount, 32);

        DrawMetaSpriteAnimation(34, (byte)(SlimeX + 24), SlimeY, SlimePaletteIndex, SlimeIdleFrames, FrameCount, 32);
    }

    static int FrameCount = 0;
    static byte ScrollX = 0;
    static byte ScrollY = 0;
    static byte ScrollNametable = 0;
    public static void Nmi()
    {
        FrameCount++;

        var colorAnimFrame = FrameCount % 48;
        if (colorAnimFrame < 8)
            SetBackgroundPaletteColors(3, 0x_0F, 0x_17, 0x_17, 0x_0F);
        else if (colorAnimFrame < 16)
            SetBackgroundPaletteColors(3, 0x_0F, 0x_07, 0x_17, 0x_0F);
        else if (colorAnimFrame < 24)
            SetBackgroundPaletteColors(3, 0x_0F, 0x_17, 0x_17, 0x_0F);
        else
            SetBackgroundPaletteColors(3, 0x_0F, 0x_27, 0x_17, 0x_0F);
    }
}