using NesAsm.Emulator;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests;

internal class UpFolderScript : NesScript
{
    public static void ProcU()
    {
        LDXi(25);
    }
}
