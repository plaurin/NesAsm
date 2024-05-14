using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MacroScript>]
[Script]
internal class CallingOtherMacroScript : NesScript
{
    public void Main()
    {
        MacroScript.MacroNoParam();
    }
}
