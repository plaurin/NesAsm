using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[HeaderSegment(mapper: 2, prgRomBanks: 2)]
[VectorsSegment()]
[StartupSegment()]
internal class StartupScript : NesScript
{
    public void Main() // TODO All mmembers should be static!
    {
        LDA(25);
    }
}
