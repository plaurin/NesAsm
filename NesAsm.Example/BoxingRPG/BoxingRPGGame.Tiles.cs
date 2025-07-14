namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    //const byte HudXTile = 0x29;
    //const byte HudPalette = 2;

    //static readonly byte[] HudCoinTiles = [0x24, 0x24, 0x24, 0x2E];
    //const byte HudCoinPalette = 3;

    const byte SkyTile = 0x24;
    static readonly byte[] SkyTiles = [SkyTile, SkyTile, SkyTile, SkyTile];
    const byte SkyPalette = 0;

    //static readonly byte[] BrickTiles = [0x45, 0x45, 0x47, 0x47];
    //const byte BrickPalette = 1;

    const byte GroundTile = 0x25;
    static readonly byte[] GroundTiles = [GroundTile, GroundTile, GroundTile, GroundTile];
    const byte GroundPalette = 1;

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