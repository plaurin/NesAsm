namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    private const byte BackgroundColor = 0x29;

    static readonly byte[,] BoxerTiles = { { 0x80, 0x81, 0x82 }, { 0x90, 0x91, 0x92 }, { 0xA0, 0xA1, 0xA2 }, { 0xB0, 0xB1, 0xB2 }, { 0xC0, 0xC1, 0xC2 }, { 0xD0, 0xD1, 0xD2 }, { 0xE0, 0xE1, 0xE2 }, { 0xF0, 0xF1, 0xF2 } };
    static byte[] BoxerPalette = [0x_0F, 0x2A, 0x_37];
    const byte BoxerPaletteIndex = 2;

    //const byte HudXTile = 0x29;
    //const byte HudPalette = 2;

    //static readonly byte[] HudCoinTiles = [0x24, 0x24, 0x24, 0x2E];
    //const byte HudCoinPalette = 3;

    const byte SkyTile = 0x26;
    static readonly byte[] SkyTiles = [SkyTile, SkyTile, SkyTile, SkyTile];
    static byte[] SkyPalette = [0x_0F, 0x30, 0x_12];
    const byte SkyPaletteIndex = 1;

    //static readonly byte[] BrickTiles = [0x45, 0x45, 0x47, 0x47];
    //const byte BrickPalette = 1;

    const byte GroundTile = 0x24;
    static readonly byte[] GroundTiles = [GroundTile, GroundTile, GroundTile, GroundTile];
    static byte[] GroundPalette = [0x_29, 0x29, 0x_29];
    const byte GroundPaletteIndex = 0;

    //static readonly byte[] QuestionTiles = [0x53, 0x54, 0x55, 0x56];
    //const byte QuestionPalette = 3;

    static readonly byte[] EmptyTiles = [0x57, 0x58, 0x59, 0x5A];
    const byte EmptyPalette = 3;

    //static readonly byte[] BlockTiles = [0xAB, 0xAD, 0xAC, 0xAE];
    //const byte BlockPalette = 1;

    //static readonly byte[] PoleTopTiles = [0x24, 0x24, 0x2F, 0x3D];
    //static readonly byte[] PoleTiles = [0xA2, 0xA3, 0xA2, 0xA3];
    //const byte FlagPalette = 0;

    //static readonly byte[] PipeTopLeftTiles = [0x60, 0x61, 0x64, 0x65];
    //static readonly byte[] PipeTopRightTiles = [0x62, 0x63, 0x66, 0x67];
    //static readonly byte[] PipeTubeLeftTiles = [0x68, 0x69, 0x68, 0x69];
    //static readonly byte[] PipeTubeRightTiles = [0x26, 0x6A, 0x26, 0x6A];
    //const byte PipePalette = 0;

    //static readonly byte[][] PipeStretchTiles =
    //[
    //    PipeTopLeftTiles, EmptyTiles, PipeTopRightTiles,
    //    PipeTubeLeftTiles, EmptyTiles, PipeTubeRightTiles,
    //    PipeTubeLeftTiles, EmptyTiles, PipeTubeRightTiles
    //];

    static readonly byte[] BushLeftTiles = [SkyTile, SkyTile, SkyTile, 0x35];
    static readonly byte[] BushTiles = [0x36, 0x37, 0x25, 0x25];
    static readonly byte[] BushRightTiles = [SkyTile, SkyTile, 0x38, SkyTile];
    const byte BushPaletteIndex = 3;

    static readonly byte[] HillLeftTiles = [SkyTile, 0x30, 0x30, 0x25];
    static readonly byte[] HillEmptyTiles = [0x25, 0x25, 0x25, 0x25];
    static readonly byte[] HillSpotTiles = [0x25, 0x34, 0x25, 0x25];
    static readonly byte[] HillTopTiles = [SkyTile, SkyTile, 0x31, 0x32];
    static readonly byte[] HillRightTiles = [0x33, SkyTile, 0x25, 0x33];
    static byte[] HillPalette = [0x_0F, 0x1A, 0x_12]; // 0x_29, 0x_1A, 0X_0F
    const byte HillPaletteIndex = 3;

    static readonly byte[] CloudTopLeftTiles = BushLeftTiles;
    static readonly byte[] CloudTopTiles = BushTiles;
    static readonly byte[] CloudTopRightTiles = BushRightTiles;
    static readonly byte[] CloudBottomLeftTiles = [SkyTile, 0x39, SkyTile, SkyTile];
    static readonly byte[] CloudBottomTiles = [0x3A, 0x3B, SkyTile, SkyTile];
    static readonly byte[] CloudBottomRightTiles = [0x3C, SkyTile, SkyTile, SkyTile];
    const byte CloudPaletteIndex = SkyPaletteIndex;

    //static readonly byte[] CastleBrick = [0x47, 0x47, 0x47, 0x47];
    //static readonly byte[] CastleRempart = [0x9D, 0x9E, 0x47, 0x47];
    //static readonly byte[] CastleRempartFront = [0xA9, 0xAA, 0x47, 0x47];
    //static readonly byte[] CastleWindowRight = [0x47, 0x27, 0x47, 0x27];
    //static readonly byte[] CastleWindowLeft = [0x27, 0x47, 0x27, 0x47];
    //static readonly byte[] CastleDoorTop = [0x9B, 0x9C, 0x27, 0x27];
    //static readonly byte[] CastleDoorBottom = [0x27, 0x27, 0x27, 0x27];
    //const byte CastlePalette = 1;

    static readonly byte[][] CloudStretchTiles =
    [
        CloudTopLeftTiles, CloudTopTiles, CloudTopRightTiles,
        EmptyTiles, EmptyTiles, EmptyTiles,
        CloudBottomLeftTiles, CloudBottomTiles, CloudBottomRightTiles
    ];
}