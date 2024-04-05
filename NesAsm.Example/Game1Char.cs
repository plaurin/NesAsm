using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Example;

[ImportChar("game1.png")]
//[ImportChar("game1.png", GridWith1pxSpacing)]
public partial class Game1Char : CharDefinition
{
    // [ImportPalettes("game1.png", "Palettes")]
    // private readonly byte[] Palettes;

    private const byte RightTile = 1;
    private const byte LeftTile = 2;
}