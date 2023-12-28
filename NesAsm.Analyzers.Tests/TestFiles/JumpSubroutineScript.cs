using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class JumpSubroutineScript : ScriptBase
{
    public JumpSubroutineScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void ProcA()
    {
        ProcB();
        ProcC();
    }

    public void ProcB()
    {
        LDAi(1);
    }

    public void ProcC()
    {
        LDAi(2);
    }
}
