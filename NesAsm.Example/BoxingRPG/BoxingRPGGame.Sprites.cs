namespace NesAsm.Example.JumpMan;

public static partial class BoxingRPGGame
{
    static readonly byte[,] SlimeIdle1 = { { 0x70, 0x71 }, { 0x72, 0x73 } };
    static readonly byte[,] SlimeIdle2 = { { 0x71, 0x70 }, { 0x73, 0x72 } };
    static readonly byte[,] SlimeShadow = { { 0x74, 0x75 } };

    static readonly MetaSpriteFrame[] SlimeIdleFrames =
    [
        new(16, SlimeIdle1),
        new(16, SlimeIdle2, flipHorizontals: new bool[,] { { true, true }, { true, true } })
    ];

    static byte[] SlimePalette = [0x_1C, 0x_16, 0x_20];
    const byte SlimePaletteIndex = 3;

    static byte[] ShadowPalette = [0x_0F, 0x_0F, 0x_0F];
    const byte ShadowPaletteIndex = 2;
}