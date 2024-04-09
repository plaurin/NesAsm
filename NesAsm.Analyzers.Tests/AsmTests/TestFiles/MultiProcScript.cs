using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class MultiProcScript : ScriptBase
{
    public MultiProcScript(NESEmulator emulator) : base(emulator)
    {
    }

    public static readonly byte Data = 0xFF;

    public void ProcA()
    {
        LDAi(10);
    }

    public void ProcB()
    {
        LDAi(20);
    }

    public void ProcC()
    {
        LDAi(30);
    }
}
