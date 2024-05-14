using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class BranchScript : NesScript
{
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
