using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

[Script]
internal class MacroScript : FileBasedReference
{
    [Macro]
    public static void MacroNoParam()
    {
        MacroWithParams(5, 10, 0x4000);
    }

    [Macro]
    public static void MacroWithParams(byte a, byte b, ushort c)
    {
        MacroOneParam((ushort)(c + b * 0x20 + a));
    }

    [Macro]
    public static void MacroOneParam(ushort a)
    {
        BIT(PPUSTATUS);
        LDA(a / 256);
        STA(PPUADDR);
        LDA(a % 256);
        STA(PPUADDR);
    }
}
