using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class SubroutineResultScript : FileBasedReference
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
