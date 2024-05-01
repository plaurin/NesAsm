using NesAsm.Emulator;

namespace NesAsm.Analyzers.Tests;

internal class UpFolderScript : ScriptBase
{
    public UpFolderScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void ProcU()
    {
        LDXi(25);
    }
}
