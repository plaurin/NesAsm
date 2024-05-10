using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MacroScript>]
internal class CallingOtherMacroScript : ScriptBase
{
    public CallingOtherMacroScript(NESEmulator emulator) : base(emulator)
    {
    }

    public void Main()
    {
        Macro<MacroScript>(s => s.MacroNoParam());
    }
}
