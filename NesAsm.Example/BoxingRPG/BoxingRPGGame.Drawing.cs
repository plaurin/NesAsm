using NesAsm.Emulator;
using static NesAsm.Emulator.PPUApiCSharp;

namespace NesAsm.Example.BoxingRPG;

public static partial class BoxingRPGGame
{
    private static void SetBackgroundPaletteColors(int index, byte[] colors)
    {
        if (colors.Length != 3)
        {
            throw new ArgumentException("Sprite palette must have exactly 3 colors.");
        }

        var color0 = index == 0 ? BackgroundColor : (byte)0x_0F; // Use BackgroundColor for index 0, otherwise use 0x_0F

        PPUApiCSharp.SetBackgroundPaletteColors(index, color0, colors[0], colors[1], colors[2]);
    }

    private static void SetSpritePaletteColors(int index, byte[] colors)
    {
        if (colors.Length != 3)
        {
            throw new ArgumentException("Sprite palette must have exactly 3 colors.");
        }

        PPUApiCSharp.SetSpritePaletteColors(index, 0x_0F, colors[0], colors[1], colors[2]);
    }

    private static void DrawTile(int x, int y, byte tileIndex)
    {
        //SetNametableTile((x % 64) / 32, x % 32, y, tileIndex);
        SetNametableTile((y % 60) / 30 * 2, x % 32, y % 30, tileIndex);
    }

    private static void SetPalette(int x, int y, byte paletteIndex)
    {
        //SetAttributeTablePalette((x % 32) / 16, x % 16, y, paletteIndex);
        SetAttributeTablePalette((y % 30) / 15 * 2, x % 16, y % 15, paletteIndex);
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

    static int LastDrawnColumn = 255;
    static readonly byte[][] ColumnBlocks = new byte[15][];
    static readonly byte[] ColumnPalettes = new byte[15];

    private static void DrawMetaTile(byte x, byte y, byte palette, byte[,] tileIndexes)
    {
        for (int i = 0; i < tileIndexes.GetLength(1); i++)
            for (int j = 0; j < tileIndexes.GetLength(0); j++)
            {
                DrawTile(x + i, y + j, tileIndexes[j, i]);
                SetPalette((x + i) / 2, (y + j) / 2, palette);
            }

    }
}