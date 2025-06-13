namespace NesAsm.Example.JumpMan;

public static partial class JumpManGame
{
    static readonly byte[] MarioStanding = [0x3A, 0x37, 0x4F, 0x4F];
    const byte MarioPalette = 0;

    static readonly byte[] GoombaWalk = [0x70, 0x71, 0x72, 0x73];
    const byte GoombaPalette = 3;
}