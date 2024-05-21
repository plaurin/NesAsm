using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.CharTests.TestFiles;

[ImportChar(@"DefinedPalette.png")]
[SpritePalette(0, 2, "Blue")]
[SpritePalette(paletteIndex: 2, tileIndex: 1)]
[SpritePalette(tileIndex: 3, name: "Green", paletteIndex: 1)]
[BackgroundPalette(name: "Purple", paletteIndex: 0, tileIndex: 2)]
[BackgroundPalette(tileIndex: 1, paletteIndex: 2)]
[BackgroundPalette(1, 3, "Green")]
public partial class DefinedPalette : CharDefinition
{
}
