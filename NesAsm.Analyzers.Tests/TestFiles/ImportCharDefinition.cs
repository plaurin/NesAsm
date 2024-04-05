using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[ImportChar(@"ImportChar.png")]
public partial class ImportCharDefinition : CharDefinition
{
    private const byte RightTile = 1;
    private const byte LeftTile = 2;
}