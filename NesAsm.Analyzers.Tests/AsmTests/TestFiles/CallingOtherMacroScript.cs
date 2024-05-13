using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MacroScript>]
[Script]
internal class CallingOtherMacroScript : FileBasedReference
{
    public void Main()
    {
        MacroScript.MacroNoParam();
    }
}
