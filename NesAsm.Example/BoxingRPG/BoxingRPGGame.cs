using NesAsm.Emulator;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.BoxingRPG;
public static partial class BoxingRPGGame
{
    const byte SplitSkyGround = 96 - 16;
    const byte SkyScrollMax = 48;
    const byte HeaderHeight = 32;
    const byte FooterHeight = 32;
    const byte SplitSkyGroundMemory = SplitSkyGround + HeaderHeight + SkyScrollMax;

    public static void Reset(CancellationToken? cancellationToken = null)
    {
        SetGameHeader(isVerticalMirroring: false);

        // Background palette
        SetBackgroundPaletteColors(GroundPaletteIndex, BoxerPalette); // And ground
        SetBackgroundPaletteColors(SkyPaletteIndex, SkyPalette);
        SetBackgroundPaletteColors(HillPaletteIndex, HillPalette);
        SetBackgroundPaletteColors(HudPaletteIndex, HudPalette);

        SetSpritePaletteColors(ShadowPaletteIndex, ShadowPalette);
        SetSpritePaletteColors(SlimePaletteIndex, SlimePalette);

        // Init;
        byte[] importColors = [0x_22, 0x_2D, 0x_10, 0x_20];
        LoadImage(@"BoxingRPG\BoxingRPG.png", importColors, hasTileSeparator: false);

        LoadFightWith("Slime", "Field");

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        // Debug
        DrawWord(0, 19, "PING");
        DrawWord(0, 20, "PONG");
        SetPalette(0, 10, HillPaletteIndex);

        // Draw Top HUD
        DrawBlockFill(0, 0, 16, 2, HudTiles, HudPaletteIndex);

        DrawWord(3, 2, "SLIME");
        DrawWord(10, 2, "HP");
        //DrawNumber(10, 3, Scroll);

        // Draw Bottom HUD
        DrawBlockFill(0, 18, 16, 12, HudTiles, HudPaletteIndex);

        DrawWord(3, 37, "HERO");
        DrawWord(10, 37, "HP");
        DrawNumber(10, 38, 99);

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

    //static int BoxerX = 0;
    //static int BoxerY = 0;
    static int SlimeX = 0;
    static byte SlimeY = 0;
    static byte SlimeShadowY = 0;
    static int SlimeJumpVelocity = 0;
    static int SlimeJumpDelta = 0;

    static byte DebugCount = 0;

    private static void MainLoop()
    {
        // Scroll to draw static HUD -> Header
        SetIRQAtScanline(HeaderHeight);
        SetScrollPosition(0, 0, 0);

        // Update HUD

        // Input Update
        Boxer.Update();

        if (InputManager.B) SlimeX -= 1;
        if (InputManager.A) SlimeX += 1;
        if (InputManager.Select && SlimeJumpDelta == 0) SlimeJumpVelocity = 4;
        if (InputManager.Start) DebugCount += 1;

        if (WindowY < 0) WindowY = 0;
        if (WindowY > SkyScrollMax) WindowY = SkyScrollMax;

        // Update Game States
        WindowX = Boxer.X / 4;

        SlimeJumpDelta += SlimeJumpVelocity;
        if (SlimeJumpDelta > 0 && FrameCount % 4 == 0) SlimeJumpVelocity--;
        if (SlimeJumpDelta <= 0) { SlimeJumpVelocity = 0; SlimeJumpDelta = 0; }

        SlimeShadowY = (byte)(3 + 8 + SplitSkyGround + HeaderHeight + SkyScrollMax - WindowY);
        SlimeY = (byte)(8 + SplitSkyGround + HeaderHeight + SkyScrollMax - WindowY - SlimeJumpDelta);

        // Update Sprites
        SpriteIndex = 0;

        // Update Slime
        var windowSlimeX = 128 + (SlimeX - Boxer.X) / 2;
        DrawMetaSprite(ref SpriteIndex, (byte)(windowSlimeX + 3), SlimeShadowY, ShadowPaletteIndex, SlimeShadow, drawBehindBackground: true);

        DrawMetaSpriteAnimation(ref SpriteIndex, (byte)windowSlimeX, SlimeY, SlimePaletteIndex, SlimeIdleFrames, FrameCount, 32, drawBehindBackground: false);

        // Update HUD
        DrawNumber(10, 3, (byte)windowSlimeX, paddingZeros: 3);
        DrawNumber(4, 3, (byte)SlimeX, paddingZeros: 3);
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
            case 0: // After Header -> Sky
                ScrollX = (byte)(WindowX % 256);
                ScrollY = (byte)(WindowY % 240);

                ScrollNametable = (byte)(((WindowY / 240) % 2) * 2);
                SetScrollPosition(ScrollNametable, ScrollX, ScrollY);

                IrqCount++;
                SetIRQAtScanline((byte)(SplitSkyGroundMemory - ScrollY));
                break;
            case 1: // After Sky -> Ground + Boxer
                var windowsBoxerX = -(Boxer.X - SlimeX) / 2;
                SetScrollPosition(0, (byte)windowsBoxerX, (byte)(ScrollY + Boxer.Y + (SkyScrollMax - ScrollY) / 4));

                DrawNumber(10, 38, (byte)windowsBoxerX, paddingZeros: 3);
                DrawNumber(4, 38, (byte)Boxer.X, paddingZeros: 3);

                IrqCount++;
                SetIRQAtScanline(240 - FooterHeight);
                break;
            case 2: // After Ground -> Footer
                SetScrollPosition(0, 123, FooterHeight + SkyScrollMax);

                IrqCount = 0;
                break;
        }
    }

    public enum BoxerStates { Idle, JumpLeft, JumpRight }
    public static BoxerStates BoxerState = BoxerStates.Idle;
    public static int BoxerAnimFrames;
}

public interface IState
{
    virtual void Enter() { }
    virtual void Exit() { }
    virtual void Update() { }
}

