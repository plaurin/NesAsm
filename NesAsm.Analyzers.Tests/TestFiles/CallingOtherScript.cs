using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class CallingOtherScript : ScriptBase
{
    public CallingOtherScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        Call<MultiProcScript>(s => s.ProcB());

        MyProcC();

        Jump<MultiProcScript>(s => s.ProcC());
    }

    public void MyProcC()
    {
        LDAi(30);
    }
}
