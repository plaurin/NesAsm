using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

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
        LDAi(0b_0001_1000);
        LDAi(0x10);

        //LDAi(5); // Enable Sprites & Background - Inline comment not supported yet

        LDA(0x3F);
        LDA(0x2002);
        LDA(PPUSTATUS);

        LDA(Data, X);
        LDA(Data, Y);
        STA(0x20, X);
        STA(0x200, Y);

        STA(0x2006);
        STA(PPUCTRL);
        STA(PPUMASK);
        STA(PPUSTATUS);
        STA(PPUADDR);
        STA(PPUDATA);

        INC(0x200);
        DEC(0x200);
        INX();
        DEX();

        LSR();
        ROL(0x20);
        SEI();
        CLD();
        TXS();
        BIT(0x2002);

        ANDi(0b_0101_0010);

        CMP(0x30);

        label:
        if (BEQ()) goto label;
        if (BNE()) goto label;
        if (BCC()) goto label;
        if (BPL()) goto label;
    }

    [RomData]
    private readonly byte[] Data = [0, 1, 2, 3, 4];
}
