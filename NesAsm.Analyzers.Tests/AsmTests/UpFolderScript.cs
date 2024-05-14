using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests;

[Script]
internal class UpFolderScript : NesScript
{
    public static void ProcU()
    {
        LDXi(25);
    }
}
