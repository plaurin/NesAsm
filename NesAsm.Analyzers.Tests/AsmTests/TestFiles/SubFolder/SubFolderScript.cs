using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests.AsmTests.TestFiles.SubFolder;

internal class SubFolderScript : ScriptBase
{
    public SubFolderScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void ProcS()
    {
        LDXi(25);
    }
}
