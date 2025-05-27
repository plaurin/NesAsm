using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.JumpMan;

public static class JumpManGame
{
    const int SkyTile = 0x24;

    static readonly byte[] BrickTiles = [0x45, 0x45, 0x47, 0x47];
    const byte BrickPalette = 1;

    static readonly byte[] GroundTiles = [0xB4, 0xB5, 0xB6, 0xB7];
    const byte GroundPalette = 1;

    static readonly byte[] QuestionTiles = [0x53, 0x54, 0x55, 0x56];
    const byte QuestionPalette = 3;

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

        // Full sky
        for (int j = 0; j < 30; j++)
            for (int i = 0; i < 32; i++)
            {
                SetNametableTile(0, i, j, SkyTile);
            }

        // Hide sprites
        for (byte i = 0; i < 64; i++)
        {
            SetSpriteData(i, 0, 250, 0, 0, false, false, false);
        }

        DrawBlockLine(0, 13, 16, GroundTiles, GroundPalette);
        DrawBlockLine(0, 14, 16, GroundTiles, GroundPalette);

        DrawBlock(5, 9, BrickTiles, BrickPalette);
        DrawBlock(6, 9, QuestionTiles, QuestionPalette);
        DrawBlock(7, 9, BrickTiles, BrickPalette);
        DrawBlock(8, 9, QuestionTiles, QuestionPalette);
        DrawBlock(9, 9, BrickTiles, BrickPalette);

        DrawBlock(7, 5, QuestionTiles, QuestionPalette);

        while (true)
        {
            MainLoop();
            WaitForVBlank();
        }
    }

    private static void MainLoop()
    {
    }

    public static void Nmi()
    {
    }

    private static void DrawBlock(int x, int y, byte[] tileIndexes, byte backgroundPalette)
    {
        if (tileIndexes.Length != 4)
        {
            throw new ArgumentException("Invalid number of tile indexes provided. Must be 1 or 4.");
        }

        SetNametableTile(0, x * 2, y * 2, tileIndexes[0]);
        SetNametableTile(0, 1 + x * 2, y * 2, tileIndexes[1]);
        SetNametableTile(0, x * 2, 1 + y * 2, tileIndexes[2]);
        SetNametableTile(0, 1 + x * 2, 1 + y * 2, tileIndexes[3]);
        SetAttributeTablePalette(0, x, y, backgroundPalette);
    }

    private static void DrawBlockLine(int x, int y, byte length, byte[] tileIndexes, byte backgroundPalette)
    {
        for (int i = 0; i < length; i++) DrawBlock(x + i, y, tileIndexes, backgroundPalette);
    }

    private enum TilePattern
    {
        FourContiguous = 0,
        UpAndDown = 1
    }
}