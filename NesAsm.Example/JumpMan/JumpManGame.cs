using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static class JumpManGame
{
    const int SkyTile = 0x24;
    static readonly byte[] SkyTiles = [SkyTile, SkyTile, SkyTile, SkyTile];
    const byte SkyPalette = 0;

    static readonly byte[] BrickTiles = [0x45, 0x45, 0x47, 0x47];
    const byte BrickPalette = 1;

    static readonly byte[] GroundTiles = [0xB4, 0xB5, 0xB6, 0xB7];
    const byte GroundPalette = 1;

    static readonly byte[] QuestionTiles = [0x53, 0x54, 0x55, 0x56];
    const byte QuestionPalette = 3;

    static readonly byte[] EmptyTiles = [0x57, 0x58, 0x59, 0x5A];
    const byte EmptyPalette = 3;

    static readonly byte[] PipeTopLeftTiles = [0x60, 0x61, 0x64, 0x65];
    static readonly byte[] PipeTopRightTiles = [0x62, 0x63, 0x66, 0x67];
    static readonly byte[] PipeTubeLeftTiles = [0x68, 0x69, 0x68, 0x69];
    static readonly byte[] PipeTubeRightTiles = [0x26, 0x6A, 0x26, 0x6A];
    const byte PipePalette = 0;

    static readonly byte[][] PipeStretchTiles =
    [
        PipeTopLeftTiles, EmptyTiles, PipeTopRightTiles,
        PipeTubeLeftTiles, EmptyTiles, PipeTubeRightTiles,
        PipeTubeLeftTiles, EmptyTiles, PipeTubeRightTiles
    ];

    static readonly byte[] BushLeftTiles = [0x24, 0x24, 0x24, 0x35];
    static readonly byte[] BushTiles = [0x36, 0x37, 0x25, 0x25];
    static readonly byte[] BushRightTiles = [0x24, 0x24, 0x38, 0x24];
    const byte BushPalette = 0;

    static readonly byte[] HillLeftTiles = [0x24, 0x30, 0x30, 0x26];
    static readonly byte[] HillEmptyTiles = [0x26, 0x26, 0x26, 0x26];
    static readonly byte[] HillSpotTiles = [0x26, 0x34, 0x26, 0x26];
    static readonly byte[] HillTopTiles = [0x24, 0x24, 0x31, 0x32];
    static readonly byte[] HillRightTiles = [0x33, 0x24, 0x26, 0x33];
    const byte HillPalette = 0;

    static readonly byte[] CloudTopLeftTiles = BushLeftTiles;
    static readonly byte[] CloudTopTiles = BushTiles;
    static readonly byte[] CloudTopRightTiles = BushRightTiles;
    static readonly byte[] CloudBottomLeftTiles = [0x24, 0x39, 0x24, 0x24];
    static readonly byte[] CloudBottomTiles = [0x3A, 0x3B, 0x24, 0x24];
    static readonly byte[] CloudBottomRightTiles = [0x3C, 0x24, 0x24, 0x24];
    const byte CloudPalette = 2;

    static readonly byte[][] CloudStretchTiles =
    [
        CloudTopLeftTiles, CloudTopTiles, CloudTopRightTiles,
        EmptyTiles, EmptyTiles, EmptyTiles,
        CloudBottomLeftTiles, CloudBottomTiles, CloudBottomRightTiles
    ];

    public static void Reset()
    {
        // Background palette
        SetBackgroundPaletteColors(0, 0x_22, 0x_29, 0x_1A, 0X_0F);
        SetBackgroundPaletteColors(1, 0x_0F, 0x_36, 0x_17, 0x_0F);
        SetBackgroundPaletteColors(2, 0x_0F, 0x_30, 0x_21, 0x_0F);
        SetBackgroundPaletteColors(3, 0x_0F, 0x_27, 0x_17, 0x_0F);

        SetSpritePaletteColors(0, 0x_22, 0x_16, 0x_27, 0x_18);

        // Init;
        LoadImage(@"JumpMan\JumpMan.png", hasTileSeparator: false);

        LoadLevel("1-1");

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        //// Full sky
        //DrawBlockFill(0, 0, 32, 15, SkyTiles, SkyPalette);

        //DrawBlockRow(0, 13, 16, GroundTiles, GroundPalette);
        //DrawBlockRow(0, 14, 16, GroundTiles, GroundPalette);

        //DrawBlock(1, 9, EmptyTiles, EmptyPalette);

        //DrawBlock(5, 9, BrickTiles, BrickPalette);
        //DrawBlock(6, 9, QuestionTiles, QuestionPalette);
        //DrawBlock(7, 9, BrickTiles, BrickPalette);
        //DrawBlock(8, 9, QuestionTiles, QuestionPalette);
        //DrawBlock(9, 9, BrickTiles, BrickPalette);

        //DrawBlock(7, 5, QuestionTiles, QuestionPalette);

        //DrawBlockStretch(13, 11, 2, 2, PipeStretchTiles, PipePalette);

        //DrawBlock(1, 12, HillLeftTiles, HillPalette);
        //DrawBlock(2, 12, HillTiles, HillPalette);
        //DrawBlock(3, 12, HillRightTiles, HillPalette);
        //DrawBlock(2, 11, HillTopTiles, HillPalette);

        //DrawBlock(8, 12, BushLeftTiles, BushPalette);
        //DrawBlock(9, 12, BushTiles, BushPalette);
        //DrawBlock(10, 12, BushRightTiles, BushPalette);

        //DrawBlock(4, 2, CloudTopLeftTiles, CloudPalette);
        //DrawBlock(5, 2, CloudTopTiles, CloudPalette);
        //DrawBlock(6, 2, CloudTopRightTiles, CloudPalette);
        //DrawBlock(4, 3, CloudBottomLeftTiles, CloudPalette);
        //DrawBlock(5, 3, CloudBottomTiles, CloudPalette);
        //DrawBlock(6, 3, CloudBottomRightTiles, CloudPalette);

        //DrawBlockStretch(12, 3, 5, 2, CloudStretchTiles, CloudPalette);

        //SetScrollPosition(1, 0, 0);

        for (int i = WindowX / 16; i < (WindowX / 16) + 11; i++)
        {
            DrawColumn(i);
        }

        while (true)
        {
            MainLoop();
            WaitForVBlank();
        }
    }

    static int WindowX = 512;

    private static void MainLoop()
    {
        WindowX += 2;
        DrawColumn((WindowX / 16) + 10);
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

        ScrollX = (byte)(WindowX % 256);
        ScrollNametable = (byte)((WindowX / 256) % 2);
        SetScrollPosition(ScrollNametable, ScrollX, 0);
    }

    private static void DrawTile(int x, int y, byte tileIndex)
    {
        SetNametableTile((x % 64) / 32, x % 32, y, tileIndex);
    }

    private static void SetPalette(int x, int y, byte paletteIndex)
    {
        SetAttributeTablePalette((x % 32) / 16, x % 16, y, paletteIndex);
    }

    private static void DrawBlock(int x, int y, byte[] tileIndexes, byte backgroundPalette)
    {
        if (tileIndexes.Length != 4)
        {
            throw new ArgumentException("Invalid number of tile indexes provided. Must be 1 or 4.");
        }

        DrawTile(x * 2, y * 2, tileIndexes[0]);
        DrawTile(1 + x * 2, y * 2, tileIndexes[1]);
        DrawTile(x * 2, 1 + y * 2, tileIndexes[2]);
        DrawTile(1 + x * 2, 1 + y * 2, tileIndexes[3]);
        SetPalette(x, y, backgroundPalette);
    }

    private static void DrawBlockRow(int x, int y, int length, byte[] tileIndexes, byte backgroundPalette)
    {
        for (int i = 0; i < length; i++) DrawBlock(x + i, y, tileIndexes, backgroundPalette);
    }

    private static void DrawBlockColumn(int x, int y, int length, byte[] tileIndexes, byte backgroundPalette)
    {
        for (int i = 0; i < length; i++) DrawBlock(x, y + i, tileIndexes, backgroundPalette);
    }

    private static void DrawBlockFill(int x, int y, int width, int height, byte[] tileIndexes, byte backgroundPalette)
    {
        for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) DrawBlock(x + i, y + j, tileIndexes, backgroundPalette);
    }

    private static void DrawBlockStretch(int leftX, int topY, int width, int height, byte[][] tileIndexes, byte backgroundPalette)
    {
        DrawBlock(leftX, topY, tileIndexes[0], backgroundPalette);
        DrawBlockRow(leftX + 1, topY, width - 2, tileIndexes[1], backgroundPalette);
        DrawBlock(leftX + width - 1, topY, tileIndexes[2], backgroundPalette);

        DrawBlockColumn(leftX, topY + 1, height - 2, tileIndexes[3], backgroundPalette);
        DrawBlockFill(leftX + 1, topY + 1, width - 2, height - 2, tileIndexes[4], backgroundPalette);
        DrawBlockColumn(leftX + width - 1, topY + 1, height - 2, tileIndexes[5], backgroundPalette);

        DrawBlock(leftX, topY + height - 1, tileIndexes[6], backgroundPalette);
        DrawBlockRow(leftX + 1, topY + height - 1, width - 2, tileIndexes[7], backgroundPalette);
        DrawBlock(leftX + width - 1, topY + height - 1, tileIndexes[8], backgroundPalette);
    }

    static int LastDrawnColumn = 255;
    static readonly byte[][] ColumnBlocks = new byte[15][];
    static readonly byte[] ColumnPalettes = new byte[15];
    private static void DrawColumn(int x)
    {
        if (x == LastDrawnColumn) return;
        LastDrawnColumn = x;

        // Default to sky
        for (int i = 0; i < 15; i++)
        {
            ColumnBlocks[i] = SkyTiles;
            ColumnPalettes[i] = SkyPalette;
        }

        // Repeating patterns
        foreach (var (pattern, index) in BackgroundPatterns)
        {
            if (x % PatternLegth == index)
            {
                switch (pattern)
                {
                    case MapPattern.HillStart:
                        ColumnBlocks[12] = HillLeftTiles;
                        ColumnPalettes[12] = HillPalette;
                        break;
                    case MapPattern.HillLeft1:
                        ColumnBlocks[11] = HillLeftTiles;
                        ColumnBlocks[12] = HillSpotTiles;
                        ColumnPalettes[11] = HillPalette;
                        ColumnPalettes[12] = HillPalette;
                        break;
                    case MapPattern.HillTop1:
                        ColumnBlocks[11] = HillTopTiles;
                        ColumnBlocks[12] = HillSpotTiles;
                        ColumnPalettes[11] = HillPalette;
                        ColumnPalettes[12] = HillPalette;
                        break;
                    case MapPattern.HillTop2:
                        ColumnBlocks[10] = HillTopTiles;
                        ColumnBlocks[11] = HillSpotTiles;
                        ColumnBlocks[12] = HillEmptyTiles;
                        ColumnPalettes[10] = HillPalette;
                        ColumnPalettes[11] = HillPalette;
                        ColumnPalettes[12] = HillPalette;
                        break;
                    case MapPattern.HillRight1:
                        ColumnBlocks[11] = HillRightTiles;
                        ColumnBlocks[12] = HillSpotTiles;
                        ColumnPalettes[11] = HillPalette;
                        ColumnPalettes[12] = HillPalette;
                        break;
                    case MapPattern.HillEnd:
                        ColumnBlocks[12] = HillRightTiles;
                        ColumnPalettes[12] = HillPalette;
                        break;

                    case MapPattern.BushStart:
                        ColumnBlocks[12] = BushLeftTiles;
                        ColumnPalettes[12] = BushPalette;
                        break;
                    case MapPattern.BushFull:
                        ColumnBlocks[12] = BushTiles;
                        ColumnPalettes[12] = BushPalette;
                        break;
                    case MapPattern.BushEnd:
                        ColumnBlocks[12] = BushRightTiles;
                        ColumnPalettes[12] = BushPalette;
                        break;
                }
            }
        }

        foreach (var (pattern, index, height) in BackgroundPatternsHeight)
        {
            if (x % PatternLegth == index)
            {
                switch (pattern)
                {
                    case MapPattern.CloudStart:
                        ColumnBlocks[height] = CloudTopLeftTiles;
                        ColumnBlocks[height + 1] = CloudBottomLeftTiles;
                        ColumnPalettes[height] = CloudPalette;
                        ColumnPalettes[height + 1] = CloudPalette;
                        break;
                    case MapPattern.CloudFull:
                        ColumnBlocks[height] = CloudTopTiles;
                        ColumnBlocks[height + 1] = CloudBottomTiles;
                        ColumnPalettes[height] = CloudPalette;
                        ColumnPalettes[height + 1] = CloudPalette;
                        break;
                    case MapPattern.CloudEnd:
                        ColumnBlocks[height] = CloudTopRightTiles;
                        ColumnBlocks[height + 1] = CloudBottomRightTiles;
                        ColumnPalettes[height] = CloudPalette;
                        ColumnPalettes[height + 1] = CloudPalette;
                        break;
                }
            }
        }

        // Repeating patterns with heights


        // Ground
        ColumnBlocks[13] = GroundTiles;
        ColumnPalettes[13] = GroundPalette;
        ColumnBlocks[14] = GroundTiles;
        ColumnPalettes[14] = GroundPalette;

        // Draw
        for (int i = 0; i < 15; i++)
        {
            DrawBlock(x, i, ColumnBlocks[i], ColumnPalettes[i]);
        }
    }

    enum MapPattern
    {
        HillStart,
        HillLeft1,
        HillTop1,
        HillTop2,
        HillRight1,
        HillEnd,

        BushStart,
        BushFull,
        BushEnd,

        CloudStart,
        CloudFull,
        CloudEnd,
    }

    static List<(MapPattern, int)> BackgroundPatterns = [];
    static List<(MapPattern, int, int)> BackgroundPatternsHeight = [];
    static int PatternLegth;

    private static void LoadLevel(string level)
    {
        PatternLegth = 48;
        
        SetBigHill(0);
        SetSmallHill(17);
        SetBush(12, 3);
        SetBush(24, 1);
        SetBush(42, 2);
        SetCloud(9, 1, 3);
        SetCloud(20, 1, 2);
        SetCloud(28, 3, 3);
        SetCloud(37, 2, 2);
    }

    private static void SetBigHill(int x) => SetBackgroundPattern(x, MapPattern.HillStart, MapPattern.HillLeft1, MapPattern.HillTop2, MapPattern.HillRight1, MapPattern.HillEnd);
    private static void SetSmallHill(int x) => SetBackgroundPattern(x, MapPattern.HillStart, MapPattern.HillTop1, MapPattern.HillEnd);
    private static void SetBush(int x, int length) => SetBackgroundPatterns(x, length, MapPattern.BushStart, MapPattern.BushFull, MapPattern.BushEnd);
    private static void SetCloud(int x, int length, int height) => SetBackgroundPatternsHeight(x, length, height, MapPattern.CloudStart, MapPattern.CloudFull, MapPattern.CloudEnd);

    private static void SetBackgroundPattern(int x, params MapPattern[] patterns)
    {
        for (int i = 0; i < patterns.Length; i++) BackgroundPatterns.Add((patterns[i], x + i));
    }

    private static void SetBackgroundPatternHeight(int x, int height, params MapPattern[] patterns)
    {
        for (int i = 0; i < patterns.Length; i++) BackgroundPatternsHeight.Add((patterns[i], x + i, height));
    }

    private static void SetBackgroundPatterns(int x, int length, MapPattern patternStart, MapPattern patternMiddle, MapPattern patternEnd)
    {
        SetBackgroundPattern(x, patternStart);
        for (int i = 0; i < length; i++) BackgroundPatterns.Add((patternMiddle, x + i + 1));
        SetBackgroundPattern(x + length + 1, patternEnd);
    }

    private static void SetBackgroundPatternsHeight(int x, int length, int height, MapPattern patternStart, MapPattern patternMiddle, MapPattern patternEnd)
    {
        SetBackgroundPatternHeight(x, height, patternStart);
        for (int i = 0; i < length; i++) BackgroundPatternsHeight.Add((patternMiddle, x + i + 1, height));
        SetBackgroundPatternHeight(x + length + 1, height, patternEnd);
    }
}