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
        LDA(PPUSTATUS);

        LDA(Data, X);
        LDA(Data, Y);

        STA(0x2006);
        STA(PPUADDR);
        STA(PPUDATA);

        LSR();
        ROL(0x20);

        label:
        if (BNE()) goto label;
        if (BCC()) goto label;
    }

    [RomData]
    private byte[] Data = [0, 1, 2, 3, 4];
}
