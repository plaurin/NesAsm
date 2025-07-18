namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    static readonly byte[,] BoxerIdle1 = { { 0x14, 0x00, 0x01 }, { 0x02, 0x03, 0x04 }, { 0x05, 0x06, 0x07 }, { 0x08, 0x09, 0x0A }, { 0x0B, 0x0C, 0x0D }, { 0x0E, 0x0F, 0x10 }, { 0x11, 0x12, 0x13 }, { 0x14, 0x15, 0x16 } };

    static readonly MetaSpriteFrame[] BoxerIdle =
    [
        new(8, BoxerIdle1),
        new(8, BoxerIdle1),
    ];

    static byte[] BoxerPalette = [0x_0F, 0x2A, 0x_37];
    const byte BoxerPaletteIndex = 0;

    static readonly byte[,] SlimeIdle1 = { { 0x70, 0x71 }, { 0x72, 0x73 } };
    static readonly byte[,] SlimeIdle2 = { { 0x71, 0x70 }, { 0x73, 0x72 } };

    static readonly MetaSpriteFrame[] SlimeIdleFrames =
    [
        new(16, SlimeIdle1),
        new(16, SlimeIdle2, flipHorizontals: new bool[,] { { true, true }, { true, true } })
    ];

    static byte[] SlimePalette = [0x_1C, 0x_16, 0x_20];
    const byte SlimePaletteIndex = 3;
}