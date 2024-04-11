using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.CharTests.TestFiles;

[ImportChar(@"InvalidSize.png")]
public partial class InvalidSize : CharDefinition
{
}