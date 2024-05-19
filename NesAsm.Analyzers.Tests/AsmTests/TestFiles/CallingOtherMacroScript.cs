using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Analyzers.Tests.TestFiles.MacroScript;

namespace NesAsm.Analyzers.Tests.TestFiles;

[FileInclude<MacroScript>]
internal class CallingOtherMacroScript : NesScript
{
    public static void Start()
    {
        MacroScript.MacroNoParam();

        MacroScript.MacroWithParams(4, 8, MacroScript.ADDR);
        
        MacroWithParams(5, 9, ADDR);
    }
}
