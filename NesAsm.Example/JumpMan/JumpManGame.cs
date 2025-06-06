using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static partial class JumpManGame
{
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

        // Draw first 16 columns
        for (int i = WindowX / 16; i < (WindowX / 16) + 16; i++)
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
            MainLoop();
            WaitForVBlank();
        }
    }

    static int WindowX = 0000;

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

    private static void DrawWord(int x, int y, string word)
    {
        for (int i = 0; i < word.Length; i++) DrawCharacter(x + i, y, word[i]);
    }

    private static void DrawCharacter(int x, int y, char character)
    {
        if ((character < 65 || character > 90) && character != 45) // A-Z or -
        {
            throw new ArgumentException("Letter must be between A and Z (ASCII 65 to 90) or a hyphen (-)");
        }

        DrawTile(x, y, (byte)(character == 45 ? 0x28 : 10 - 65 + character));
    }

    private static void DrawNumber(int x, int y, long number, int paddingZeros = 0)
    {
        var digits = number.ToString().PadLeft(paddingZeros, '0');
        for (int i = 0; i < digits.Length; i++) DrawDigit(x + i, y, digits[i]);
    }

    private static void DrawDigit(int x, int y, char digit)
    {
        if (digit < 48 || digit > 57) // 0-9
        {
            throw new ArgumentException("Digit must be between 0 and 9 (ASCII 48 to 57).");
        }

        DrawTile(x, y, (byte)(digit - 48));
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
        for (int i = 2; i < 15; i++)
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
        for (int i = 2; i < 15; i++)
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
}