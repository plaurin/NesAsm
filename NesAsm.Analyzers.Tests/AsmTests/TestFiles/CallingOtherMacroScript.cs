using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MacroScript>]
internal class CallingOtherMacroScript : NesScript
{
    public static void Start()
    {
        MacroScript.MacroNoParam();
    }
}
