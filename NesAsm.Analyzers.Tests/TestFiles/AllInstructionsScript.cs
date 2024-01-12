using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class AllInstructionsScript : ScriptBase
{
    public AllInstructionsScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void AllInstructions()
    {
        LDA(0x3F);
        LDA(0x2002);

        STA(0x2006);
    }
}
