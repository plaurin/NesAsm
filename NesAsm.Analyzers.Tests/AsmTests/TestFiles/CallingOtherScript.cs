using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MultiProcScript>]
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
        LDA(MultiProcScript.Data);
        LDA(MultiProcScript.SpritePalettes, X);
    }
}
