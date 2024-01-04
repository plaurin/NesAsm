using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class SubroutineResultsScript : ScriptBase
{
    public SubroutineResultsScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        // Argument of the method could not be used after calling a subroutine with return values
        var (a, b, c) = ProcB();

        LDAa(a);
        STA(0x40);

        //LDAa(b);
        //STA(0x41);

        LDAa(c);
        STA(0x42);
    }

    public (byte, ushort, bool) ProcB()
    {
        return (250, 1080, true);
    }
}
