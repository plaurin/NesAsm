using NesAsm.Emulator;
using System.Drawing;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.FPS;

public static partial class FPSGame
{
    static readonly byte[] Palette = [0x0A, 0x1A, 0x2A];
    private const byte EmptyTile = 0x24;
    private const byte XTile = 0x29;
    static readonly byte[] EmptyTiles = [EmptyTile, EmptyTile, EmptyTile, EmptyTile];

    private const byte BackgroundColor = 0x3F;
    private const byte BarSprite = 0x00;

    public static void Reset(CancellationToken? cancellationToken = null)
    {
        SetGameHeader(isVerticalMirroring: true);

        // Background palette
        SetBackgroundPaletteColors(0, Palette);

        SetSpritePaletteColors(0, Palette);

        // Init;
        byte[] importColors = [0x_22, 0x_2D, 0x_10, 0x_20];
        LoadImage(@"BoxingRPG\BoxingRPG.png", importColors, hasTileSeparator: false);

        // Empty Background
        DrawBlockFill(0, 0, 16, 15, EmptyTiles, 0);

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        // Set some Tile
        DrawTile(5, 5, XTile);
        DrawTile(6, 6, XTile);
        DrawTile(7, 7, XTile);

        SetupMap();

        // Main Loop
        while (cancellationToken == null || !cancellationToken.Value.IsCancellationRequested)
        {
            MainLoop();
            //WaitForIrq(Irq); // After Header
            //WaitForIrq(Irq); // After Sky
            //WaitForIrq(Irq); // After Ground
            WaitForVBlank();
        }
    }

    static readonly Point Player = new(0, 0);
    static byte PlayerOrientation = 0;

    private static void MainLoop()
    {
        // Scroll to draw static HUD -> Header
        //SetIRQAtScanline(HeaderHeight);
        SetScrollPosition(0, 0, 0);

        // Input
        if (InputManager.Left) PlayerOrientation -= 1;
        if (InputManager.Right) PlayerOrientation += 1;

        // Calc map point angles
        //byte i = 0;
        PointAngle.Clear();
        PointDistance.Clear();
        for (var i = 0; i < Map.Count; i++)
        {
            var point = Map[i];
            var dx = point.X - Player.X;
            var dy = point.Y - Player.Y;
            byte angle = (byte)(Math.Atan2(dy, dx) * 128 / Math.PI);

            PointAngle.Add(angle);
            PointDistance.Add((byte)Math.Sqrt(dx * dx + dy * dy));
        }

        // Display Points
        for (byte i = 0; i < PointAngle.Count; i++)
        {
            var pointAngle = PointAngle[i];
            //var dAngle = (byte)(pointAngle - PlayerOrientation);
            //if (dAngle > 128) dAngle = (byte)(256 - dAngle);

            var dAngle = (sbyte)(pointAngle - PlayerOrientation);

            bool isVisible = dAngle < 0x20 && dAngle > -0x20;
            if (isVisible)
            {
                // Draw Point
                byte midScreenX = 128;
                byte pixelPerAngle = 4;
                byte x = (byte)(midScreenX - (dAngle * pixelPerAngle));

                byte y = (byte)(200 - PointDistance[i] * 4);

                SetSpriteData(i, x, y, BarSprite, 0, false, false, false);
            }
            else
            {
                // Hide Point
                SetSpriteData(i, 0, 250, 0, 0, false, false, false);
            }

            var point = Map[i];
            DrawWord(0, i, $"POINT     ANGLE     VISIBLE");
            DrawNumber(6, i, Math.Abs(point.X));
            DrawNumber(8, i, Math.Abs(point.Y));
            DrawNumber(16, i, pointAngle);
            if (isVisible)
                DrawWord(28, i, "Y"); 
            else
                DrawWord(28, i, "N");
        }

        DrawWord(0, 24, $"PLAYER     LEFT     RIGHT");
        DrawNumber(7, 24, (byte)PlayerOrientation);
        DrawNumber(16, 24, (byte)(PlayerOrientation - 0x32));
        DrawNumber(26, 24, (byte)(PlayerOrientation + 0x32));
    }

    private static readonly List<byte> PointAngle = [];
    private static readonly List<byte> PointDistance = [];

    public static void Nmi()
    {
//        FrameCount++;
    }

    public static void Irq()
    {

    }

    private static readonly List<Point> Map = [];
    private static void SetupMap()
    {
        Map.Add(new Point(4, 4));
        Map.Add(new Point(8, 3));
        Map.Add(new Point(8, -3));
        Map.Add(new Point(4, -4));
        Map.Add(new Point(3, -8));
        Map.Add(new Point(-3, -8));
        Map.Add(new Point(-4, -4));
        Map.Add(new Point(-8, -3));
        Map.Add(new Point(-8, 3));
        Map.Add(new Point(-4, 4));
        Map.Add(new Point(-3, 8));
        Map.Add(new Point(3, 8));
    }

    private record Point(int X, int Y);
}
