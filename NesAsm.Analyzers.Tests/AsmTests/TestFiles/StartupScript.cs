using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[HeaderSegment(mapper: 2, prgRomBanks: 2)]
[VectorsSegment()]
[StartupSegment()]
[Script]
internal class StartupScript : FileBasedReference
{
    public void Main()
    {
        LDA(25);
    }
}
