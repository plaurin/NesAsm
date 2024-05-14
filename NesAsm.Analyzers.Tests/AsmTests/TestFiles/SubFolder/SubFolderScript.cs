using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.AsmTests.TestFiles.SubFolder;

[Script]
internal class SubFolderScript : NesScript
{
    public static void ProcS()
    {
        LDXi(25);
    }
}
