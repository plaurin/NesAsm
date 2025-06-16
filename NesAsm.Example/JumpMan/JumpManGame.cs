using NesAsm.Emulator;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static partial class JumpManGame
{
    public static void Reset()
    {
        SetSpriteZeroHitScanline(31);

        // Background palette
        SetBackgroundPaletteColors(0, 0x_22, 0x_29, 0x_1A, 0X_0F);
        SetBackgroundPaletteColors(1, 0x_0F, 0x_36, 0x_17, 0x_0F);
        SetBackgroundPaletteColors(2, 0x_0F, 0x_30, 0x_21, 0x_0F);
        SetBackgroundPaletteColors(3, 0x_0F, 0x_27, 0x_17, 0x_0F);

        SetSpritePaletteColors(0, 0x_22, 0x_16, 0x_27, 0x_18);
        SetSpritePaletteColors(3, 0x_0F, 0x_0F, 0x_36, 0x_17);

        // Init;
        LoadImage(@"JumpMan\JumpMan.png", hasTileSeparator: false);

        LoadLevel("1-1");

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        // Draw first screen columns ++
        for (int i = WindowX / 16; i < (WindowX / 16) + DrawAheadColumns; i++)
        {
            DrawColumn(i);
        }

        // Draw HUD
        for (int i = 0; i < 32; i++)
        {
            DrawBlock(i, 0, SkyTiles, HudPalette);
            DrawBlock(i, 1, SkyTiles, HudPalette);
        }

        DrawWord(3, 2, "MARIO");
        DrawNumber(3, 3, 300, 6);

        DrawBlock(5, 1, HudCoinTiles, HudCoinPalette);
        DrawTile(12, 3, HudXTile);
        DrawNumber(13, 3, 2, 2);

        DrawWord(18, 2, "WORLD");
        DrawDigit(19, 3, '1');
        DrawCharacter(20, 3, '-');
        DrawDigit(21, 3, '1');

        DrawWord(25, 2, "TIME");
        DrawNumber(26, 3, 297);

        // Main Loop
        while (true)
        {
            MainLoopBeforeSpriteZeroHit();
            WaitForSpriteZeroHit();
            MainLoopAfterSpriteZeroHit();
            WaitForVBlank();
        }
    }

    static int WindowX = 0000;
    const int DrawAheadColumns = 20;

    private static void MainLoopBeforeSpriteZeroHit()
    {
        // Scroll to draw static HUD
        SetScrollPosition(0, 0, 0);

        // Do light weight stuff that we are sure will complete before the hud in the top finish to draw
        DrawColumn((WindowX / 16) + DrawAheadColumns);
    }

    static byte Coins = 90;
    static int Time = 400;
    static byte MarioX = 100;
    static byte MarioY = 208;
    static byte GoombaX = 150;
    static byte GoombaY = 208;

    private static void MainLoopAfterSpriteZeroHit()
    {
        // Scroll to game position
        ScrollX = (byte)(WindowX % 256);
        ScrollNametable = (byte)((WindowX / 256) % 2);
        SetScrollPosition(ScrollNametable, ScrollX, 0);

        // Update HUD
        if (FrameCount % 15 == 0) Time--;
        if (Time < 0) Time = 0;

        if (FrameCount % 20 == 0) Coins++;
        if (Coins >= 100) Coins -= 100;

        // Coin
        DrawNumber(13, 3, Coins, 2);

        // Time
        DrawNumber(26, 3, Time);

        WindowX += 1;

        if (InputManager.Left) MarioX -= 1;
        if (InputManager.Right) MarioX += 1;
        if (InputManager.Up) MarioY -= 1;
        if (InputManager.Down) MarioY += 1;

        if (InputManager.B) GoombaX -= 1;
        if (InputManager.A) GoombaX += 1;
        if (InputManager.Select) GoombaY -= 1;
        if (InputManager.Start) GoombaY += 1;

        // Update Mario
        DrawMetaSpriteAnimation(1, MarioX, MarioY, MarioPalette, BigMarionRunning, FrameCount, 24);

        // Update Goomba
        DrawMetaSpriteAnimation(10, GoombaX, GoombaY, GoombaPalette, GoombaWalkingFrames, FrameCount, 32);

        DrawMetaSpriteAnimation(20, (byte)(GoombaX + 24), GoombaY, GoombaPalette, GoombaWalkingFrames, FrameCount, 32);
    }

    static int FrameCount = 0;
    static byte ScrollX = 0;
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