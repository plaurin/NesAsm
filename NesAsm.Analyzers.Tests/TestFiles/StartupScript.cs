using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

[HeaderSegment(mapper: 2, prgRomBanks: 2)]
[VectorsSegment()]
[StartupSegment()]
internal class StartupScript : ScriptBase
{
    public StartupScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        LDA(25);
    }
}
