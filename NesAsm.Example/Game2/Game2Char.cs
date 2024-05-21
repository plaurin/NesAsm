using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Example.Game2;

[ImportChar("game2.png")]
// TODO ImportChar without grid 128x256
[SpritePalette(paletteIndex: 0, tileIndex: 9, name: "Face")]
[SpritePalette(paletteIndex: 0, tileIndex: 10, name: "Hearth")]
[BackgroundPalette(paletteIndex: 0, tileIndex: 2, name: "Ground")]
[BackgroundPalette(paletteIndex: 1, tileIndex: 3, name: "Brick")]
public partial class Game2Char : CharDefinition
{
    public const byte RightTile = 1;
    public const byte LeftTile = 2;
    public const byte DownTile = 3;
    public const byte UpTile = 4;
    public const byte StartTile = 5;
    public const byte SelectTile = 6;
    public const byte BTile = 7;
    public const byte ATile = 8;
    public const byte FaceTile = 9;
    public const byte HearthTile = 10;
}