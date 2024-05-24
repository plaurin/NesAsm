using NesAsm.Emulator;
using NesAsm.Emulator.Attributes;
using static NesAsm.Emulator.NESEmulatorStatic;

namespace NesAsm.Analyzers.Tests.TestFiles;

internal class MacroScript : NesScript
{
    public const ushort ADDR = 0x2000;

    [Macro]
    public static void MacroNoParam()
    {
        MacroWithParams(5, 10, 0x4000);
    }

    [Macro]
    public static void MacroWithParams(byte a, byte b, ushort c)
    {
        MacroOneParam((ushort)(c + b * 0x20 + a));

        // TODO Fix arg usage in macro should use the arg name directly?
        LDAa(a);
        LDXa(b);
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

    [Macro]
    public static void MacroWithLoops()
    {
        for (X = 0; X < 4; X++)
        {
            for (Y = 0; Y < 4; Y++)
            {
                LDAi(25);
            }
        }

        for (X = 0; X < 2; X++)
        {
            for (Y = 0; Y < 2; Y++)
            {
                LDAi(52);
            }
            for (Y = 0; Y < 6; Y++)
            {
                LDAi(2);
            }
        }
    }
}
