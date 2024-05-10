using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class MacroScript : ScriptBase
{
    public MacroScript(NESEmulator emulator) : base(emulator)
    {
    }

    [Macro]
    public void MacroNoParam()
    {
        Macro<MacroScript>(s => s.MacroWithParams(5, 10, 0x4000));
    }

    [Macro]
    public void MacroWithParams(byte a, byte b, ushort c)
    {
        MacroOneParam((ushort)(c + b * 0x20 + a));
    }

    [Macro]
    public void MacroOneParam(ushort a)
    {
        BIT(PPUSTATUS);
        LDA(a / 256);
        STA(PPUADDR);
        LDA(a % 256);
        STA(PPUADDR);
    }
}
