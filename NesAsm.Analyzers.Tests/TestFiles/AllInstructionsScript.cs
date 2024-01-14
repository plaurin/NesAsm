using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class AllInstructionsScript : ScriptBase
{
    public AllInstructionsScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void AllInstructions()
    {
        LDAi(2);
        LDXi(3);
        LDYi(4);

        LDA(0x3F);
        LDA(0x2002);

        LDA(Data, X);
        LDA(Data, Y);

        STA(0x2006);
    }

    [RomData]
    private byte[] Data = [0, 1, 2, 3, 4];
}
