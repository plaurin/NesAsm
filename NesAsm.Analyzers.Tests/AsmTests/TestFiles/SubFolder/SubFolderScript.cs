using NesAsm.Emulator;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.AsmTests.TestFiles.SubFolder;

internal class SubFolderScript : NesScript
{
    public static void ProcS()
    {
        LDXi(25);
    }
}
