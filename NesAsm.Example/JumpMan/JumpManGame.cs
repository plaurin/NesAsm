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

    static readonly byte[] BlockTiles = [0xAB, 0xAD, 0xAC, 0xAE];
    const byte BlockPalette = 1;

    static readonly byte[] PoleTopTiles = [0x24, 0x24, 0x2F, 0x3D];
    static readonly byte[] PoleTiles = [0xA2, 0xA3, 0xA2, 0xA3];
    const byte FlagPalette = 0;

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

    static readonly byte[] CastleBrick = [0x47, 0x47, 0x47, 0x47];
    static readonly byte[] CastleRempart = [0x9D, 0x9E, 0x47, 0x47];
    static readonly byte[] CastleRempartFront = [0xA9, 0xAA, 0x47, 0x47];
    static readonly byte[] CastleWindowRight = [0x47, 0x27, 0x47, 0x27];
    static readonly byte[] CastleWindowLeft = [0x27, 0x47, 0x27, 0x47];
    static readonly byte[] CastleDoorTop = [0x9B, 0x9C, 0x27, 0x27];
    static readonly byte[] CastleDoorBottom = [0x27, 0x27, 0x27, 0x27];
    const byte CastlePalette = 1;

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


        for (int i = WindowX / 16; i < (WindowX / 16) + 16; i++)
        {
            DrawColumn(i);
        }

        while (true)
        {
            MainLoop();
            WaitForVBlank();
        }
    }

    static int WindowX = 3000;

    private static void MainLoop()
    {
        WindowX += 1;
        DrawColumn((WindowX / 16) + 16);
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
            SetColumnBlocks(i, SkyPalette, SkyTiles);
        }

        // Repeating patterns
        foreach (var (pattern, index) in BackgroundPatterns)
        {
            if (x % PatternLegth == index)
            {
                switch (pattern)
                {
                    case MapPattern.HillStart:
                        SetColumnBlocks(12, HillPalette, HillLeftTiles);
                        break;
                    case MapPattern.HillLeft1:
                        SetColumnBlocks(11, HillPalette, HillLeftTiles, HillSpotTiles);
                        break;
                    case MapPattern.HillTop1:
                        SetColumnBlocks(11, HillPalette, HillTopTiles, HillSpotTiles);
                        break;
                    case MapPattern.HillTop2:
                        SetColumnBlocks(10, HillPalette, HillTopTiles, HillSpotTiles, HillEmptyTiles);
                        break;
                    case MapPattern.HillRight1:
                        SetColumnBlocks(11, HillPalette, HillRightTiles, HillSpotTiles);
                        break;
                    case MapPattern.HillEnd:
                        SetColumnBlocks(12, HillPalette, HillRightTiles);
                        break;

                    case MapPattern.BushStart:
                        SetColumnBlocks(12, BushPalette, BushLeftTiles);
                        break;
                    case MapPattern.BushFull:
                        SetColumnBlocks(12, BushPalette, BushTiles);
                        break;
                    case MapPattern.BushEnd:
                        SetColumnBlocks(12, BushPalette, BushRightTiles);
                        break;
                }
            }
        }

        // Repeating patterns with heights
        foreach (var (pattern, index, height) in BackgroundPatternsHeight)
        {
            if (x % PatternLegth == index)
            {
                switch (pattern)
                {
                    case MapPattern.CloudStart:
                        SetColumnBlocks(height, CloudPalette, CloudTopLeftTiles, CloudBottomLeftTiles);
                        break;
                    case MapPattern.CloudFull:
                        SetColumnBlocks(height, CloudPalette, CloudTopTiles, CloudBottomTiles);
                        break;
                    case MapPattern.CloudEnd:
                        SetColumnBlocks(height, CloudPalette, CloudTopRightTiles, CloudBottomRightTiles);
                        break;
                }
            }
        }

        // Ground
        FillColumnBlocks(13, 2, GroundPalette, GroundTiles);

        // Foreground elements
        foreach (var (pattern, index, height) in ForegroundElements)
        {
            if (x == index)
            {
                switch (pattern)
                {
                    case MapPattern.CoinBox:
                        SetColumnBlocks(height, QuestionPalette, QuestionTiles);
                        break;
                    case MapPattern.Brick:
                        SetColumnBlocks(height, BrickPalette, BrickTiles);
                        break;
                    case MapPattern.EmptyBox:
                        SetColumnBlocks(height, EmptyPalette, EmptyTiles);
                        break;
                    case MapPattern.Block:
                        FillColumnBlocks(13 - height, height, BlockPalette, BlockTiles);
                        break;

                    case MapPattern.PipeLeft:
                        FillColumnBlocks(13 - height, height, PipePalette, PipeTopLeftTiles, PipeTubeLeftTiles);
                        break;
                    case MapPattern.PipeRight:
                        FillColumnBlocks(13 - height, height, PipePalette, PipeTopRightTiles, PipeTubeRightTiles);
                        break;

                    case MapPattern.Hole:
                        FillColumnBlocks(13, 2, SkyPalette, SkyTiles);
                        break;

                    case MapPattern.Flag:
                        FillColumnBlocks(2, 10, FlagPalette, PoleTopTiles, PoleTiles);
                        break;

                    case MapPattern.CastleWall:
                        SetColumnBlocks(10, CastlePalette, CastleRempart, CastleBrick, CastleBrick);
                        break;
                    case MapPattern.CastleWallLvl2Left:
                        SetColumnBlocks(8, CastlePalette, CastleRempart, CastleWindowRight, CastleRempartFront, CastleBrick, CastleBrick);
                        break;
                    case MapPattern.CastleWallLvl2Right:
                        SetColumnBlocks(8, CastlePalette, CastleRempart, CastleWindowLeft, CastleRempartFront, CastleBrick, CastleBrick);
                        break;
                    case MapPattern.CastleDoor:
                        SetColumnBlocks(8, CastlePalette, CastleRempart, CastleBrick, CastleRempartFront, CastleDoorTop, CastleDoorBottom);
                        break;
                }
            }
        }


        // Draw
        for (int i = 0; i < 15; i++)
        {
            DrawBlock(x, i, ColumnBlocks[i], ColumnPalettes[i]);
        }
    }

    private static void SetColumnBlocks(int y, byte paletteIndex, params byte[][] tileIndexes)
    {
        for (int i = 0; i < tileIndexes.Length; i++)
        {
            ColumnBlocks[y + i] = tileIndexes[i];
            ColumnPalettes[y + i] = paletteIndex;
        }
    }

    private static void FillColumnBlocks(int y, int length, byte paletteIndex, byte[] tileIndex)
    {
        for (int i = 0; i < length; i++)
        {
            ColumnBlocks[y + i] = tileIndex;
            ColumnPalettes[y + i] = paletteIndex;
        }
    }

    private static void FillColumnBlocks(int y, int length, byte paletteIndex, byte[] startTileIndex, byte[]? fillTileIndex)
    {
        ColumnBlocks[y] = startTileIndex;
        for (int i = 1; i < length; i++) ColumnBlocks[y + i] = fillTileIndex!;

        for (int i = 0; i < length; i++) ColumnPalettes[y + i] = paletteIndex;
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

        CoinBox,
        Brick,
        EmptyBox,
        Block,

        PipeLeft,
        PipeRight,

        Hole,

        Flag,

        CastleWall,
        CastleWallLvl2Left,
        CastleWallLvl2,
        CastleWallLvl2Right,
        CastleDoor
    }

    static List<(MapPattern, int)> BackgroundPatterns = [];
    static List<(MapPattern, int, int)> BackgroundPatternsHeight = [];
    static int PatternLegth;

    static List<(MapPattern, int, int)> ForegroundElements = [];

    private static void LoadLevel(string level)
    {
        // Repeating background patterns
        PatternLegth = 48;

        SetBigHill(0);
        SetCloud(8, 1, 3);
        SetBush(11, 3);
        SetSmallHill(16);
        SetCloud(19, 1, 2);
        SetBush(23, 1);
        SetCloud(27, 3, 3);
        SetCloud(36, 2, 2);
        SetBush(41, 2);

        // Forground map elements
        SetForegroundElement(16, 9, MapPattern.CoinBox);

        SetForegroundElement(20, 9, MapPattern.Brick);
        SetForegroundElement(21, 9, MapPattern.CoinBox);
        SetForegroundElement(22, 5, MapPattern.CoinBox);
        SetForegroundElement(22, 9, MapPattern.Brick);
        SetForegroundElement(23, 9, MapPattern.CoinBox);
        SetForegroundElement(24, 9, MapPattern.Brick);
        
        SetForegroundElement(28, 2, MapPattern.PipeLeft);
        SetForegroundElement(29, 2, MapPattern.PipeRight);

        SetForegroundElement(38, 3, MapPattern.PipeLeft);
        SetForegroundElement(39, 3, MapPattern.PipeRight);

        SetForegroundElement(46, 4, MapPattern.PipeLeft);
        SetForegroundElement(47, 4, MapPattern.PipeRight);

        SetForegroundElement(57, 4, MapPattern.PipeLeft);
        SetForegroundElement(58, 4, MapPattern.PipeRight);

        SetForegroundElement(64, 8, MapPattern.CoinBox);
        
        SetForegroundElement(69, 0, MapPattern.Hole);
        SetForegroundElement(70, 0, MapPattern.Hole);

        SetForegroundElement(77, 9, MapPattern.Brick);
        SetForegroundElement(78, 9, MapPattern.CoinBox);
        SetForegroundElement(79, 9, MapPattern.Brick);

        SetForegroundElement(80, 5, MapPattern.Brick);
        SetForegroundElement(81, 5, MapPattern.Brick);
        SetForegroundElement(82, 5, MapPattern.Brick);
        SetForegroundElement(83, 5, MapPattern.Brick);
        SetForegroundElement(84, 5, MapPattern.Brick);
        SetForegroundElement(85, 5, MapPattern.Brick);
        SetForegroundElement(86, 5, MapPattern.Brick);
        SetForegroundElement(86, 0, MapPattern.Hole);
        SetForegroundElement(87, 5, MapPattern.Brick);
        SetForegroundElement(87, 0, MapPattern.Hole);

        SetForegroundElement(91, 5, MapPattern.Brick);
        SetForegroundElement(92, 5, MapPattern.Brick);
        SetForegroundElement(93, 5, MapPattern.Brick);
        SetForegroundElement(94, 5, MapPattern.CoinBox);
        SetForegroundElement(94, 9, MapPattern.CoinBox);

        SetForegroundElement(100, 9, MapPattern.Brick);
        SetForegroundElement(101, 9, MapPattern.CoinBox);
        
        SetForegroundElement(106, 9, MapPattern.CoinBox);
        SetForegroundElement(109, 5, MapPattern.CoinBox);
        SetForegroundElement(109, 9, MapPattern.CoinBox);
        SetForegroundElement(112, 9, MapPattern.CoinBox);

        SetForegroundElement(118, 9, MapPattern.Brick);
        
        SetForegroundElement(121, 5, MapPattern.Brick);
        SetForegroundElement(122, 5, MapPattern.Brick);
        SetForegroundElement(123, 5, MapPattern.Brick);
        
        SetForegroundElement(128, 5, MapPattern.Brick);
        SetForegroundElement(129, 5, MapPattern.CoinBox);
        SetForegroundElement(129, 9, MapPattern.Brick);
        SetForegroundElement(130, 5, MapPattern.CoinBox);
        SetForegroundElement(130, 9, MapPattern.Brick);
        SetForegroundElement(131, 5, MapPattern.Brick);
        
        SetForegroundElement(134, 1, MapPattern.Block);
        SetForegroundElement(135, 2, MapPattern.Block);
        SetForegroundElement(136, 3, MapPattern.Block);
        SetForegroundElement(137, 4, MapPattern.Block);

        SetForegroundElement(140, 4, MapPattern.Block);
        SetForegroundElement(141, 3, MapPattern.Block);
        SetForegroundElement(142, 2, MapPattern.Block);
        SetForegroundElement(143, 1, MapPattern.Block);

        SetForegroundElement(148, 1, MapPattern.Block);
        SetForegroundElement(149, 2, MapPattern.Block);
        SetForegroundElement(150, 3, MapPattern.Block);
        SetForegroundElement(151, 4, MapPattern.Block);
        SetForegroundElement(152, 4, MapPattern.Block);

        SetForegroundElement(153, 0, MapPattern.Hole);
        SetForegroundElement(154, 0, MapPattern.Hole);

        SetForegroundElement(155, 4, MapPattern.Block);
        SetForegroundElement(156, 3, MapPattern.Block);
        SetForegroundElement(157, 2, MapPattern.Block);
        SetForegroundElement(158, 1, MapPattern.Block);

        SetForegroundElement(163, 2, MapPattern.PipeLeft);
        SetForegroundElement(164, 2, MapPattern.PipeRight);

        SetForegroundElement(168, 9, MapPattern.Brick);
        SetForegroundElement(169, 9, MapPattern.Brick);
        SetForegroundElement(170, 9, MapPattern.CoinBox);
        SetForegroundElement(171, 9, MapPattern.Brick);

        SetForegroundElement(179, 2, MapPattern.PipeLeft);
        SetForegroundElement(180, 2, MapPattern.PipeRight);

        SetForegroundElement(181, 1, MapPattern.Block);
        SetForegroundElement(182, 2, MapPattern.Block);
        SetForegroundElement(183, 3, MapPattern.Block);
        SetForegroundElement(184, 4, MapPattern.Block);
        SetForegroundElement(185, 5, MapPattern.Block);
        SetForegroundElement(186, 6, MapPattern.Block);
        SetForegroundElement(187, 7, MapPattern.Block);
        SetForegroundElement(188, 8, MapPattern.Block);
        SetForegroundElement(189, 8, MapPattern.Block);
        
        SetForegroundElement(198, 0, MapPattern.Flag);
        SetForegroundElement(198, 1, MapPattern.Block);
        
        SetForegroundElement(202, 0, MapPattern.CastleWall);
        SetForegroundElement(203, 0, MapPattern.CastleWallLvl2Left);
        SetForegroundElement(204, 0, MapPattern.CastleDoor);
        SetForegroundElement(205, 0, MapPattern.CastleWallLvl2Right);
        SetForegroundElement(206, 0, MapPattern.CastleWall);
    }

    #region Background Patterns

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

    #endregion

    #region Foreground map elements

    private static void SetForegroundElement(int x, int height, MapPattern mapPattern) => ForegroundElements.Add((mapPattern, x, height));

    #endregion
}