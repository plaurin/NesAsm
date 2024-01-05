using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class SubroutineResultScript : ScriptBase
{
    public SubroutineResultScript(NESEmulator emulator) : base(emulator)
    {
    }

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
