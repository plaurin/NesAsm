using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class BranchScript : ScriptBase
{
    public BranchScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        LDXi(0);

        loop:

        STX(1);
        INX();

        CPXi(4);

        if (BNE()) goto loop;

        // Memory location 0x01 should be equals to 3
    }
}
