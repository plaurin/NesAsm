namespace NesAsm.Example.JumpMan;

public static partial class JumpManGame
{
    static readonly byte[,] MarioStanding = { { 0x3A, 0x37 }, { 0x4F, 0x4F } };
    static readonly bool[,] MarioStandingFlipH = { { false, false }, { false, true} };

    static readonly byte[,] BigMarioRunning1 = { { 0x00, 0x01 }, { 0x02, 0x03 }, { 0x04, 0x05 }, { 0x06, 0x07 } };
    static readonly byte[,] BigMarioRunning2 = { { 0x08, 0x09 }, { 0x0A, 0x0B }, { 0x0C, 0x0D }, { 0x0E, 0x0F } };
    static readonly byte[,] BigMarioRunning3 = { { 0x10, 0x11 }, { 0x12, 0x13 }, { 0x14, 0x15 }, { 0x16, 0x17 } };

    static readonly MetaSpriteFrame[] BigMarionRunning =
    [
        new(8, BigMarioRunning1),
        new(8, BigMarioRunning2),
        new(8, BigMarioRunning3)
    ];

    const byte MarioPalette = 0;

    static readonly byte[] GoombaWalk = [0x70, 0x71, 0x72, 0x73];

    static readonly byte[,] GoombaWalking1 = { { 0x70, 0x71 }, { 0x72, 0x73 } };
    static readonly byte[,] GoombaWalking2 = { { 0x70, 0x71 }, { 0x73, 0x72 } };
    static readonly bool[,] GoombaWalking2FlipH = { { false, false }, { true, true } };

    static readonly MetaSpriteFrame[] GoombaWalkingFrames =
    [
        new(16, GoombaWalking1),
        new(16, GoombaWalking2, flipHorizontals: GoombaWalking2FlipH)
    ];

    const byte GoombaPalette = 3;
}