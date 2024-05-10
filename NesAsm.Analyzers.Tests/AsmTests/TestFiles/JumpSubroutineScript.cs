using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class JumpSubroutineScript : FileBasedReference
{
    public void ProcA()
    {
        ProcB();
        ProcC();
    }

    public void ProcB()
    {
        LDAi(1);

        ProcC();
    }

    [NoReturnProc]
    public static void ProcC()
    {
        LDAi(2);

        JumpInsideProc();
    }

    [NoReturnProc]
    public static void JumpInsideProc()
    {
        loop:

        LDAi(2);

        goto loop;
    }
}
