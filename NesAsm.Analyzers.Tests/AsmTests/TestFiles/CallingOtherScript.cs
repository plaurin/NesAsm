using NesAsm.Analyzers.Tests.AsmTests.TestFiles.SubFolder;
using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MultiProcScript>]
[FileInclude<UpFolderScript>("..")]
[FileInclude<SubFolderScript>("SubFolder")]
internal class CallingOtherScript : NesScript
{
    public static void Start()
    {
        MultiProcScript.ProcB();

        MyProcC();

        MultiProcScript.ProcC();
    }

    public static void MyProcC()
    {
        LDA(MultiProcScript.Data);
        LDA(MultiProcScript.SpritePalettes, X);

        UpFolderScript.ProcU();
        SubFolderScript.ProcS();
    }
}
