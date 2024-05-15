using NesAsm.Emulator;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class SubroutineResultScript : NesScript
{
    public void Main()
    {
        // Argument of the method could not be used after calling a subroutine with return values
        var a = ProcB();

        LDAa(a);
        STA(0x40);
    }

    public byte ProcB()
    {
        return 178;
    }
}
